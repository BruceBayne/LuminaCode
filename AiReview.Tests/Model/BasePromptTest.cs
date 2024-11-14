using System.Text.RegularExpressions;

namespace AiReview.Tests.Model
{
	public abstract class BasePromptTest
	{
		private static Dictionary<string, object> SummaryToDict(string summaryContent)
		{
			var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			// Regex pattern to match the key-value pairs (e.g., MinRating: 0)
			const string keyValuePattern = @"(\w+):\s*([^,\n]+(?:,\s*[^,\n]+)*)";
			var matches = Regex.Matches(summaryContent, keyValuePattern);
			foreach (Match match in matches)
			{
				string key = match.Groups[1].Value;
				string valueStr = match.Groups[2].Value.Trim();


				// Check for string array (comma-separated values)


				// Try to parse the value as a number if possible, otherwise keep it as a string
				if (int.TryParse(valueStr, out var intValue))
				{
					result[key] = intValue;
				}
				else if (double.TryParse(valueStr, out var doubleValue))
				{
					result[key] = doubleValue;
				}
				else if (valueStr.StartsWith("[") && valueStr.EndsWith("]"))
				{
					string trimmedValue = valueStr.Trim('[', ']');
					result[key] = trimmedValue.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
				}
				else
				{
					result[key] = valueStr;
				}
			}

			return result;
		}

		protected static async Task<TestSnippet> ExtractSnippet(string fileName, string testName)
		{
			var sourcePath = Path.Combine(TestCases.GetTestCasesFolder(), fileName);
			var sourceContent = await File.ReadAllTextAsync(sourcePath);

			// Define a pattern to match all summary blocks with optional Name field
			var summaryPattern = @"/// <summary>(?s)(.*?)/// </summary>";

			// Match all summary blocks
			var summaryMatches = Regex.Matches(sourceContent, summaryPattern, RegexOptions.Singleline);

			if (summaryMatches.Count == 0)
				throw new Exception("No summary block found");

			// Extract all summaries with their corresponding source code
			var summaries = new List<Tuple<string, string>>();

			for (int i = 0; i < summaryMatches.Count; i++)
			{
				var summaryContent = summaryMatches[i].Groups[1].Value.Trim();

				// Determine the source code based on whether there is a next summary
				string sourceCode = string.Empty;
				if (i + 1 < summaryMatches.Count)
				{
					// Source code is the content between this summary and the next one
					sourceCode = sourceContent.Substring(summaryMatches[i].Index + summaryMatches[i].Length,
						summaryMatches[i + 1].Index - (summaryMatches[i].Index + summaryMatches[i].Length)).Trim();
				}
				else
				{
					// If it's the last summary, source code is everything after the last summary block
					sourceCode = sourceContent.Substring(summaryMatches[i].Index + summaryMatches[i].Length).Trim();
				}

				summaries.Add(new(summaryContent, sourceCode));
			}

			// Filter based on testName if provided
			var targetSummary = string.IsNullOrEmpty(testName)
				? summaries.First() // If testName is empty, return the first summary
				: summaries.FirstOrDefault(s =>
					Regex.IsMatch(s.Item1, $@"(?:Name\s*:\s*{Regex.Escape(testName)}\s*)",
						RegexOptions.IgnoreCase)); // Match by Name

			if (targetSummary == null)
				throw new Exception($"No summary block with Name '{testName}' found");

			// Parse the summary content into a dictionary (Values)
			var parsedDict = SummaryToDict(targetSummary.Item1);
			if (!string.IsNullOrEmpty(testName)) parsedDict["TestName"] = testName;

			// Return the TestSnippet with the parsed data
			return new TestSnippet
			{
				Values = parsedDict,
				RawContent = sourceContent,
				SourceCode = targetSummary.Item2,
			};
		}
	}
}
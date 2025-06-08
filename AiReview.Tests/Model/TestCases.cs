using System.Reflection;
using System.Text.RegularExpressions;

namespace AiReview.Tests.Model;

public sealed class TestCases : TheoryData<string, string>
{
	public TestCases()
	{
		var folderPath = GetTestCasesFolder();

		if (!Directory.Exists(folderPath))
			throw new DirectoryNotFoundException($"TestCases folder not found: {folderPath}");

		foreach (var filePath in Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories))
		{
			var sourceContent = File.ReadAllText(filePath);

			// Define a pattern to match all summary blocks with optional Name field
			const string summaryPattern = @"/// <summary>(?s)(.*?)/// </summary>";

			// Match all summary blocks
			var summaryMatches = Regex.Matches(sourceContent, summaryPattern, RegexOptions.Singleline);

			// Match all <summary> blocks

			var tests = new List<(string, string)>();

			foreach (Match match in summaryMatches)
			{
				// Extract the matched block
				var summaryBlock = match.Value;

				// Try to find the Name key inside the block
				var nameMatch = Regex.Match(summaryBlock, @"///\s*Name\s*:\s*(.*?)\s*(?:\n|$)");

				// Extract the Name if it exists, otherwise default to an empty string
				var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "";

				// Normalize the file path
				var normalizedPath = filePath
					.Replace($"{folderPath}{Path.DirectorySeparatorChar}", string.Empty)
					.Replace("\\", "/");

				// Add to your collection
				tests.Add((normalizedPath,name));
			
			}

			var arr=tests.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToArray();

			foreach (var (a, b) in arr)
			{
				Add(a,b);
			}

		}
	}

	internal static string GetTestCasesFolder()
	{
		var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var folderPath = Path.Combine(assemblyDirectory, "TestCases");
		return folderPath;
	}
}
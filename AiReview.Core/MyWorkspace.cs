using AiReview.Core.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AiReview.Core.OpenAI;

namespace AiReview.Core;

public class AiResponseConverter
{
	private static T? GetValue<T>(JObject jObject, params string[] aliases)
	{
		foreach (var alias in aliases)
		{
			if (jObject.TryGetValue(alias, StringComparison.InvariantCultureIgnoreCase, out var jt))
			{
				return jt.Value<T>();
			}
		}

		return default;
	}


	private static IEnumerable<T> GetValues<T>(JObject jObject, params string[] aliases)
	{
		foreach (var alias in aliases)
		{
			if (jObject.TryGetValue(alias, StringComparison.InvariantCultureIgnoreCase, out var jt))
			{
				if (jt.Type == JTokenType.Array)
					return jt.Values<T>()!;
			}
		}

		return default;
	}

	private static string ExtractJson(string input)
	{
		var match = Regex.Match(input, @"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}");
		return match.Success ? match.Value : string.Empty;
	}

	// Function to extract the additional explanation (non-JSON part)
	private static string ExtractExplanation(string input, string jsonPart)
	{
		// Remove the JSON part from the input to get the explanation
		return input.Replace(jsonPart, "").Trim();
	}


	public static CodeReviewSummary ToReviewSummary(ChatCompletionResponse aiResponse)
	{
		var textResponse = aiResponse.Choices.First().Message.content;
		var model = aiResponse.Model.Replace("lm studio community/", string.Empty);

		var summary = ToReviewSummary(textResponse);
		summary.LLmProps = $"{model} / ";
		return summary;
	}

	public static CodeReviewSummary ToReviewSummary(string aiResponse)
	{
		var json = ExtractJson(aiResponse);
		try
		{
			var jObject = JObject.Parse(json);

			// Look for "rating" or "Rank" field
			var rating = GetValue<int>(jObject, "rating", "Rank", "score", "stars");
			var jIssues = GetValues<JObject>(jObject, "issues", "problems", "findings", "isues");

			var issues = jIssues.Select(issue => new CodeReviewIssue
			{
				Category = GetValue<string>(issue, "category", "type"),
				Description = GetValue<string>(issue, "description", "info", "conclusion", "message", "issue"),
			}).ToArray();


			var reviewSummary = new CodeReviewSummary
			{
				ReviewScore = rating,
				Issues = issues
			};
			return reviewSummary;
		}
		catch (Exception e)
		{
			Console.WriteLine(">>>> JSON:" + json);
			return new CodeReviewSummary
				{ Issues = Array.Empty<CodeReviewIssue>(), ReviewScore = 0, LLmProps = String.Empty };
		}
	}
}

public class MyWorkspace
{
	public const string AiPromptTemplate = """
	                                       Act as an expert code reviewer.
	                                       Review following C# code and identify significant issues in areas, performance, concurrency violations, redundancy, readability, IO operations, naming, and most common mistakes. 
	                                       Be extremely compact but keep some context, no assumptions no long explanations, no notes, no nitpicks.

	                                       Provide overall 10 star quality rating named rating, where 10 means high quality code.
	                                       Provide output as compact JSON with rating and issues array.
	                                        
	                                       """;


	public static string GetContentFromFile(string filePath, long from, int length)
	{
		try
		{
			// Open the file in read mode
			using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			// Ensure 'from' is within the bounds of the file size
			if (from < 0 || from >= fileStream.Length)
			{
				throw new ArgumentOutOfRangeException("The 'from' value is out of bounds.");
			}

			// Move the pointer to the 'from' position
			fileStream.Seek(from, SeekOrigin.Begin);

			// Read the specified length of bytes
			byte[] buffer = new byte[length];
			int bytesRead = fileStream.Read(buffer, 0, length);

			// Ensure we only return the content we've actually read
			if (bytesRead < length)
			{
				// Adjust the length to the number of bytes read if it's less than expected
				Array.Resize(ref buffer, bytesRead);
			}

			// Convert the byte array to a string
			return Encoding.UTF8.GetString(buffer);
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}
}
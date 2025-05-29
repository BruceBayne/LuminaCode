using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AiReview.Core.OpenAI;
using AiReview.Core.UI;
using Newtonsoft.Json.Linq;

namespace AiReview.Core;

public static class AiResponseConverter
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

	public static string TrimModel(string model)
	{
		var index = model.LastIndexOf('/') + 1;
		if (index > 0 && index < model.Length)
		{
			model = model.Substring(index);
		}

		return model;
	}


	public static CodeReviewSummary ToReviewSummary(this ChatCompletionResponse aiResponse)
	{
		var textResponse = aiResponse.Choices.First().Message.content;
		var model = TrimModel(aiResponse.Model);
		var summary = ToReviewSummary(textResponse);
		summary.LLmProps = $"{model}";
		return summary;
	}


	public static CodeReviewSummary Trim(this CodeReviewSummary summary)
	{
		summary.Issues = summary.Issues.Where(x => x.Severity.ToLower().Trim() != "low").ToArray();

		if (!summary.Issues.Any())
		{
			var emptySummary = CodeReviewSummary.Empty();
			emptySummary.RawResponse = summary.RawResponse;
			emptySummary.LLmProps = summary.LLmProps;
			return emptySummary;
		}

		return summary;
	}


	public static CodeReviewSummary ToReviewSummary(string aiResponse)
	{
		var json = ExtractJson(aiResponse);
		try
		{
			var jObject = JObject.Parse(json);

			var rating = GetValue<int>(jObject,"rank", "rating", "score", "stars");
			var jIssues = GetValues<JObject>(jObject, "issues", "problems", "findings", "isues");

			var issues = jIssues.Select(issue => new CodeReviewIssue
			{
				Category = GetValue<string>(issue, "category", "type", "area"),
				Context = GetValue<string>(issue, "context","lineNumber","lineNumbers","line_number","line_numbers"),
				Description = GetValue<string>(issue, "description", "info", "conclusion", "message", "issue", "overview"),
				Severity = GetValue<string>(issue, "severity", "urgency", "importance"),
				Notes = GetValue<string>(issue, "notes", "aux_info", "AuxInfo", "aux"),
			}).OrderBy(x=>x.Category).ToArray();


			var reviewSummary = new CodeReviewSummary
			{
				ReviewScore = rating,
				Issues = issues,
				RawResponse = aiResponse
			};
			return reviewSummary;
		}
		catch (Exception e)
		{
			Console.WriteLine($@"LLM Response Deserialization Exception : {e.Message} {Environment.NewLine} >>>> JSON:{json}");
			throw;
			return CodeReviewSummary.Empty();
		}
	}
}
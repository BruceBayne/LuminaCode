using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.Review;
using AiReview.Core.OpenAI;
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
        summary.RawResponse = textResponse;
        summary.LLmProps = $"{model}";
        return summary;
    }


    public static BetterNamesAnswer ToBetterNames(this ChatCompletionResponse aiResponse)
    {
        var textResponse = aiResponse.Choices.First().Message.content;
        var model = TrimModel(aiResponse.Model);
        var summary = ToBetterNames(textResponse);
        summary.RawResponse = textResponse;
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

            var rating = GetValue<int>(jObject, "rank", "rating", "score", "stars");
            var jIssues = GetValues<JObject>(jObject, "issues", "problems", "findings", "isues");

            var issues = jIssues.Select(issue => new CodeReviewIssue
            {
                Category = GetValue<string>(issue, "category", "type", "area"),
                Context =
                    GetValue<string>(issue, "context", "lineNumber", "lineNumbers", "line_number", "line_numbers"),
                Description = GetValue<string>(issue, "description", "info", "conclusion", "message", "issue",
                    "overview"),
                Severity = GetValue<string>(issue, "severity", "urgency", "importance"),
                Notes = GetValue<string>(issue, "notes", "aux_info", "AuxInfo", "aux"),
            }).OrderBy(x => x.Category).ToArray();


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
            Console.WriteLine(
                $@"LLM Response Deserialization Exception : {e.Message} {Environment.NewLine} >>>> JSON:{json}");
            throw;
            return CodeReviewSummary.Empty();
        }
    }


    public static string ExtractJsonArrayOnly(string input)
    {
        var match = Regex.Match(input, @"\[\s*(""[^""]*""\s*(,\s*""[^""]*""\s*)*)\]");
        if (match.Success)
            return match.Value;

        throw new FormatException("No valid JSON array found in the input.");
    }

    public static BetterNamesAnswer ToBetterNames(string aiResponse)
    {
        var json = ExtractJsonArrayOnly(aiResponse);
        try
        {
            var token = JToken.Parse(json);
            if (token.Type == JTokenType.Array)
            {
                var names = token.ToObject<string[]>();
                return new BetterNamesAnswer
                {
                    Names = names.Where(x => !string.IsNullOrEmpty(x) && x.Length > 1)
                        .Select(x => x.Trim())
                        .Distinct(StringComparer.InvariantCultureIgnoreCase)
                        .Take(5)
                        .ToArray()
                };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $@"LLM Response Deserialization Exception : {e.Message} {Environment.NewLine} >>>> JSON:{json}");
            throw;
        }

        return BetterNamesAnswer.Empty();
    }
}
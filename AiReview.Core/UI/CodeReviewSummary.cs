using System;
using System.Collections.Generic;
using System.Linq;

namespace AiReview.Core.UI;

[Serializable]
public class CodeReviewSummary
{
	public string LLmProps { get; set; }
	public int ReviewScore { get; set; }
	public float Temperature { get; set; }
	public string RawRequest { get; set; }
	public string RawResponse { get; set; }

	public IEnumerable<CodeReviewIssue> Issues { get; set; }
	public bool IsEmpty => Issues == null || !Issues.Any() || ReviewScore == 0;


	public bool HasOnlyMinorIssues
	{
		get
		{
			if (!Issues.Any())
				return true;

			return Issues.All(x =>
				x.Severity != null && (x.Severity.ToLower() == "low" || x.Severity.ToLower() == "none"));
		}
	}


	public override string ToString()
	{
		var itemsAsString = string.Join(Environment.NewLine, Issues.Select(b => b.ToString()));
		return
			$"Score : {ReviewScore} {Environment.NewLine} Issues({Issues.Count()}){Environment.NewLine}{itemsAsString}";
	}


	public static CodeReviewSummary Dummy = new()
	{
		Issues =
		[
			new CodeReviewIssue
			{
				Category = "CDemo",
				Description = "DNone",
				Severity = "High",
				Notes = "aux"
			}
		],
		ReviewScore = 3,
		LLmProps = "LM"
	};


	public static CodeReviewSummary Empty() => new(){ LLmProps = "empty"};


	public static CodeReviewSummary FromFastEstimation
	(
		string userPrompt,
		float temperature,
		ReviewAssistant.CodeEstimation estimation
	)
		=> new()
		{
			Issues = Array.Empty<CodeReviewIssue>(),
			ReviewScore = estimation.Rating,
			LLmProps = estimation.Model,
			RawResponse = estimation.RawResponse,
			Temperature = temperature,
			RawRequest = userPrompt,
		};
}
using System;
using System.Collections.Generic;
using AiReview.Core.UI;

namespace AiReview.Core.Database;

public sealed class ReviewReport
{
	public string Model;
	public int ReviewScore;
	public IEnumerable<CodeReviewIssue> Issues= Array.Empty<CodeReviewIssue>();

}
namespace AiReview.Core.UI;

public sealed class CodeReviewIssue
{
	public string Category { get; set; }
	
	
	public string Context { get; set; }
	public string Description { get; set; }
	public string Severity { get; set; }
	public string Notes { get; set; }

	public override string ToString() => $"Severity: {Severity} / Category:{Category} / {Description} / Aux:{Notes}";
}
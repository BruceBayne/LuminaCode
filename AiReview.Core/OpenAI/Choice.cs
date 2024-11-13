namespace AiReview.Core.OpenAI
{
	public class Choice
	{
		public string Text { get; set; }
		public int Index { get; set; }
		public object LogProbs { get; set; }
		public string FinishReason { get; set; }
	}
}
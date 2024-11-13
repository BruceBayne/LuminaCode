namespace AiReview.Core.OpenAI
{
	public class ChatChoice
	{
		public ChatMessage Message { get; set; }
		public int Index { get; set; }
		public object LogProbs { get; set; }
		public string FinishReason { get; set; }
	}
}
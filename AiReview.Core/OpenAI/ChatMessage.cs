namespace AiReview.Core.OpenAI
{
	public class ChatMessage
	{
		public ChatMessage()
		{
		}

		public ChatMessage(string role, string content)
		{
			this.role = role;
			this.content = content;
		}

		public string role { get; set; }
		public string content { get; set; }
	}
}
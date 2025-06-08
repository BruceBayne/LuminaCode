using System;
using System.Collections.Generic;

namespace AiReview.Core.OpenAI
{
	public class ChatCompletionResponse
	{
		public string Id { get; set; }
		public string Object { get; set; }
		public int Created { get; set; }
		public string Model { get; set; }
		public IEnumerable<ChatChoice> Choices { get; set; }
		public Usage Usage { get; set; }

		public TimeSpan Duration { get; set; }
	}



	

	public sealed class ModelsResponse
	{
		

		public  IEnumerable<ModelInfo> Data { get; set; }
	}
}
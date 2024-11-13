using System.Collections.Generic;

namespace AiReview.Core.OpenAI
{
	public class EmbeddingResponse
	{
		public string Object { get; set; }
		public IEnumerable<EmbeddingData> Data { get; set; }
		public Usage Usage { get; set; }
	}
}
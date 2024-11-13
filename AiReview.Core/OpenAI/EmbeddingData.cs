using System.Collections.Generic;

namespace AiReview.Core.OpenAI
{
	public class EmbeddingData
	{
		public IEnumerable<float> Embedding { get; set; }
		public string Index { get; set; }
	}
}
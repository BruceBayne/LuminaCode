using System;

namespace AiReview.Core.OpenAI
{
	public class ModelInfo
	{
		public string Id { get; set; }
		public string Object { get; set; }
		public string OwnedBy { get; set; }
		public string Root { get; set; }
		public string ParentId { get; set; }
	}
}

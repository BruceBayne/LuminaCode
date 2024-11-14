using System.Collections.Generic;

namespace AiReview.Core.Database
{
	public sealed class ReviewDatabase
	{
		public string Prompt;
		public Dictionary<string, ReviewAggregate> Elements = new();
	}
}
using System.Collections.Generic;

namespace AiReview.Core.Database;

public sealed class ReviewAggregate
{
	public string NavPath;
	public List<ReviewReport> Reports = new();
}
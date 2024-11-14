/// <summary>
/// Name : List Race Condition
/// MinRating: 1
///	MaxRating: 5
/// HumanReview: [race condition, concurrency violation]
/// </summary>

class ListRCExample
{
	static List<int> sharedList = new List<int>();

	static void AddToList()
	{
		for (int i = 0; i < 10000; i++)
		{
			sharedList.Add(i);
		}
	}

	static void Main()
	{
		Thread t1 = new Thread(AddToList);
		Thread t2 = new Thread(AddToList);

		t1.Start();
		t2.Start();

		t1.Join();
		t2.Join();

		Console.WriteLine($"List Count: {sharedList.Count}");
	}
}





/// <summary>
/// Name : DynamicExpressionsTests
/// MinRating: 8
///	MaxRating: 10
/// </summary>


using System.Collections.Generic;
using FluentAssertions;
using SDS.TopND.Domain;
using Xunit;
using SDS.TopND.Domain.Services.AutomaticProductType;

namespace SDS.TopND.Test
{
	public sealed class DynamicExpressionsTests
	{
		[Fact]
		[Trait("Category", "UnitTests")]
		public void BrokenExpressions_Are_Ignored()
			=>
				GetBrokenExpressions().Compile().Should().BeEmpty();

		private static IEnumerable<ProductTypeMatchingRule> GetBrokenExpressions()
		{
			yield return new ProductTypeMatchingRule()
			{
				CSharpExpression = "la la la"
			};

			yield return new ProductTypeMatchingRule()
			{
				CSharpExpression = ""
			};

			yield return new ProductTypeMatchingRule()
			{
				CSharpExpression = null
			};

			yield return new ProductTypeMatchingRule()
			{
				CSharpExpression = "throw new Exception();"
			};
		}
	}
}
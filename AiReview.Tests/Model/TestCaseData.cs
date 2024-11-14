using AiReview.Core;

namespace AiReview.Tests.Model;

public sealed class TestSnippet
{
	public Dictionary<string, object> Values { get; init; }

	public string RawContent { get; init; }
	public string SourceCode { get; init; }

	public int MinRating
	{
		get
		{
			if (Values.TryGetValue(nameof(MinRating), out var minRating))
			{
				return (int)minRating;
			}

			return 1;
		}
	}

	public string ReviewPrompt
	{
		get
		{
			if (!Values.TryGetValue(nameof(ReviewPrompt), out var rp))
				return "general";


			return (string)rp;
		}
	}

	public string[] HumanReview
	{
		get
		{
			if (Values.TryGetValue(nameof(HumanReview), out var value))
			{

				if (value is string[] strings)
				{
					return strings.ToArray();
				}

				return [(string)value];

			}
			return [];
		}
	}

	public int MaxRating
	{
		get
		{
			if (Values.TryGetValue(nameof(MaxRating), out var maxRating))
			{
				return (int)maxRating;
			}

			return 10;
		}
	}

	public string ExpectedOutput => (string)Values[nameof(ExpectedOutput)];

	public int Temperature
	{
		get
		{
			if (Values.TryGetValue(nameof(Temperature), out var t))
				return (int)t;

			return 0;
		}
	}

	public override string ToString()
	{
		return
			$"{nameof(MinRating)}: {MinRating}, {nameof(MaxRating)}: {MaxRating}, {nameof(ExpectedOutput)}: {ExpectedOutput}, {nameof(Temperature)}: {Temperature}";
	}
}
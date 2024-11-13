using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace AiReview.Core.UI
{
	public class StarRatingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is int score)
			{
				// Limit the score to a maximum of 5 stars
				int maxStars = Math.Min(score, 10);
				return new string('★', maxStars).PadRight(10, '☆'); // Fills up to 5 stars with empty stars
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class CodeReviewIssue
	{
		public string Category { get; set; }

		public override string ToString()
		{
			return $"Category:{Category} / {Description}";
		}

		public string Description { get; set; }
	}


	public class CodeReviewSummary
	{

		public static CodeReviewSummary Dummy = new CodeReviewSummary()
		{
			Issues = new[] { new CodeReviewIssue() { Category = "CDemo", Description = "DNone" } },
			ReviewScore = 3,
			LLmProps = "LM"
		};

		public string LLmProps { get; set; }
		public int ReviewScore { get; set; }


		public IEnumerable<CodeReviewIssue> Issues { get; set; }

		public override string ToString()
		{
			var itemsAsString = string.Join("\r\n\t", Issues.Select(b => b.ToString()));
			return
				$"Score : {ReviewScore} {Environment.NewLine} Issues({Issues.Count()}){Environment.NewLine}{itemsAsString}";
		}
	}

	public sealed class CodeLensDetailsModel : CodeReviewSummary
	{
		//public int ReviewScore { get; set; }

		//public IReadOnlyCollection<CodeReviewIssue> Issues { get; set; }
	}
}
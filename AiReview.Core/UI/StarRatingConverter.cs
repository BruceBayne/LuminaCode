using System;
using System.Windows.Data;

namespace AiReview.Core.UI;

public sealed class StarRatingConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is int score)
		{
			int maxStars = Math.Min(score, 10);
			return new string('★', maxStars).PadRight(10, '☆');
		}

		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter,
		System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}
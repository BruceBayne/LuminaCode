using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AiReview.Core.UI;

public class CategoryColorConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length > 0 && values[0] is string category)
		{
			// Map the category to a specific color
			switch (category)
			{
				case "Performance":
					return new SolidColorBrush(Color.FromRgb(255, 165, 0)); // Orange
				case "Security":
					return new SolidColorBrush(Color.FromRgb(255, 0, 0)); // Red
				case "Readability":
					return new SolidColorBrush(Color.FromRgb(0, 128, 0)); // Green
				default:
					return new SolidColorBrush(Color.FromRgb(192, 192, 192)); // Light gray
			}
		}

		return Binding.DoNothing;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
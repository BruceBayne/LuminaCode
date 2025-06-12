using System.Windows.Controls;

namespace AiReview.Core.UI
{
	/// <summary>
	/// Interaction logic for CodeLensDetailsControl.xaml
	/// </summary>
	public partial class CodeLensDetailsControl : UserControl
	{
		public CodeLensDetailsControl(ReviewDetailsModel model)
		{
			InitializeComponent();
			DataContext = model;
		}
	}
}

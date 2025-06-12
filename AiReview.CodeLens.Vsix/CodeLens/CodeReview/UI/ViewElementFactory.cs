using System.ComponentModel.Composition;
using System.Windows;
using AiReview.Core.UI;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.CodeReview
{
	[Export(typeof(IViewElementFactory))]
	[Name("XCodeLens details view element factory")]
	[TypeConversion(from: typeof(ReviewDetailsModel), to: typeof(FrameworkElement))]
	[Order]
	public class ViewElementFactory : IViewElementFactory
	{
		public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
			=> new CodeLensDetailsControl((ReviewDetailsModel)model) as TView;
	}
}

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace AiReview.CodeLens.Vsix.CodeLens.Provider
{

	



	[Export(typeof(IAsyncCodeLensDataPointProvider))]
	[Name("XCodeLensDataPointProvider")]
	[ContentType("CSharp")]
	[Priority(515)]
	public class CodeLensDataPointProvider : IAsyncCodeLensDataPointProvider
	{
		private readonly Lazy<ICodeLensCallbackService> callbackService;
		

		[ImportingConstructor]
		public CodeLensDataPointProvider()
		{
			
		}

		private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
		{
			foreach (var change in e.Changes)
			{
				// Convert the change to a text span
				var span = new TextSpan(change.OldPosition, change.OldLength);

				// Check if any of our active lenses are affected
				//foreach (var lens in _activeLenses)
				//{
				//	var lensSpan = lens.Key.GetSpan(e.After);

				//	// Check if the change overlaps with this lens's method
				//	if (SpansOverlap(span, new TextSpan(lensSpan.Start, lensSpan.Length)))
				//	{
				//		// Get the updated method syntax
				//		var document = lens.Value.Document;
				//		var root = await document.GetSyntaxRootAsync();
				//		var method = root.FindNode(lensSpan) as MethodDeclarationSyntax;

				//		if (method != null)
				//		{
				//			// Update the lens data
				//			await UpdateLensData(lens.Value, method);
				//		}
				//	}
				//}
			}
		}
		private bool SpansOverlap(TextSpan span1, TextSpan span2)
		{
			return span1.Start <= span2.End && span2.Start <= span1.End;
		}

		public Task<bool> CanCreateDataPointAsync(
			CodeLensDescriptor descriptor,
			CodeLensDescriptorContext descriptorContext,
			CancellationToken token
		)
		{
			var methodsOnly = descriptor.Kind == CodeElementKinds.Method;
			return Task.FromResult(methodsOnly);
		}


		public async Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(
			CodeLensDescriptor descriptor,
			CodeLensDescriptorContext descriptorContext,
			CancellationToken token
		)
		{

			//await callbackService.Value.InvokeAsync<bool>(this, nameof(IZalupa.IsZalupa));
	
			await Task.CompletedTask.ConfigureAwait(false);
			var dataPoint = new CodeLensDataPoint(descriptor);
			return dataPoint;
		}
	}
}
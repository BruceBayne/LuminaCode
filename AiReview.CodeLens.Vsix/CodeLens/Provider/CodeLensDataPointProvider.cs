using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.Provider
{
	[Export(typeof(IAsyncCodeLensDataPointProvider))]
	[Name("AI-Review")]
	[ContentType("CSharp")]
	[Priority(515)]
	[method: ImportingConstructor]
	public class CodeLensDataPointProvider(ICodeLensCallbackService cbs) : IAsyncCodeLensDataPointProvider
	{
		public static bool IsFunctionCtorMethodOrClass(CodeElementKinds kind)
		{
			const CodeElementKinds targetKinds = CodeElementKinds.Function | CodeElementKinds.Constructor |
			                                     CodeElementKinds.Method | CodeElementKinds.Class;

			// Perform a bitwise AND to check if the given kind matches any of the target kinds
			return (kind & targetKinds) != 0;
		}


		public Task<bool> CanCreateDataPointAsync(
			CodeLensDescriptor descriptor,
			CodeLensDescriptorContext descriptorContext,
			CancellationToken token
		)
		{
			var result = IsFunctionCtorMethodOrClass(descriptor.Kind);
			return Task.FromResult(result);
		}


		public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(
			CodeLensDescriptor descriptor,
			CodeLensDescriptorContext descriptorContext,
			CancellationToken token
		)
		{
			var dataPoint = new CodeLensDataPoint(descriptor, cbs);
			return Task.FromResult<IAsyncCodeLensDataPoint>(dataPoint);
		}
	}
}
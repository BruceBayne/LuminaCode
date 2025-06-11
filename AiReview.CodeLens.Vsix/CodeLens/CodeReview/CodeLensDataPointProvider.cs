using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.CodeReview
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name("LuminaCode Code-Review")]
    [ContentType("CSharp")]
    [Priority(515)]
    [method: ImportingConstructor]
    public class CodeReviewProvider(ICodeLensCallbackService cbs) : IAsyncCodeLensDataPointProvider
    {
        public Task<bool> CanCreateDataPointAsync(
            CodeLensDescriptor descriptor,
            CodeLensDescriptorContext descriptorContext,
            CancellationToken token
        )
        {
            var result = Ext.IsFunctionMethodOrClass(descriptor.Kind);
            return Task.FromResult(result);
        }


        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(
            CodeLensDescriptor descriptor,
            CodeLensDescriptorContext descriptorContext,
            CancellationToken token
        )
        {
            var dataPoint = new ReviewDataPoint(descriptor, cbs);
            return Task.FromResult<IAsyncCodeLensDataPoint>(dataPoint);
        }
    }
}
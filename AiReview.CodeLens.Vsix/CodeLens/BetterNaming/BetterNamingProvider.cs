using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.BetterNaming;

[Export(typeof(IAsyncCodeLensDataPointProvider))]
[Name("LuminaCode BetterNaming")]
[ContentType("CSharp")]
[Priority(516)]
[method: ImportingConstructor]
public class BetterNamingProvider(ICodeLensCallbackService cbs) : IAsyncCodeLensDataPointProvider
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


    public async Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(
        CodeLensDescriptor descriptor,
        CodeLensDescriptorContext descriptorContext,
        CancellationToken token
    )
    {

        await Task.CompletedTask.ConfigureAwait(false);
        //var opts = await BetterNamingSettingsModel.GetLiveInstanceAsync();
        var dataPoint = new BetterNamingPoint(descriptor, cbs);
        return dataPoint;
    }
}
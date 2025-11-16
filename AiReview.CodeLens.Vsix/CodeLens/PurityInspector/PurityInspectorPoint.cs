using AiReview.CodeLens.Vsix.Ai;
using AiReview.Core.LLM;
using AiReview.Core.LLM.PurityInspector;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiReview.CodeLens.Vsix.CodeLens.PurityInspector;

public class PurityInspectorPoint : IAsyncCodeLensDataPoint
{
    private readonly ICodeLensCallbackService devEnv;

    private PurityInspectionResult output = new();

    private static readonly CodeLensDetailEntryCommand refreshCmdId = new()
    {
        CommandSet = new Guid("2f0c282d-60f6-1a46-1c70-446b61f3be31"),
        CommandId = 0x0100
    };


    public CodeLensDescriptor Descriptor { get; }
    public event AsyncEventHandler InvalidatedAsync;

    public PurityInspectorPoint(CodeLensDescriptor descriptor, ICodeLensCallbackService devEnv)
    {
        this.devEnv = devEnv;
        Descriptor = descriptor;
    }


    public async Task<CodeLensDataPointDescriptor> GetDataAsync(
        CodeLensDescriptorContext ctx,
        CancellationToken token
    )
    {
        await Task.CompletedTask.ConfigureAwait(false);


        var path = Descriptor.FilePath;
        var from = ctx.ApplicableSpan.Value.Start;
        var to = ctx.ApplicableSpan.Value.Length;
        var end = ctx.ApplicableSpan.Value.End;


        var sourceCode = await devEnv.InvokeAsync<string>(this, nameof(IAiReviewService.ExtractSourceCode),
            [path, from, end], cancellationToken: token);

        var config = await devEnv.InvokeAsync<LuminaCodeProjectOptions>(this,
            nameof(IAiReviewService.GetProjectOptions),
            [Descriptor.FilePath], token);

        if (!config.PureInspectorOptions.IsEnabled)
        {
            return new CodeLensDataPointDescriptor
            {
                Description = "λ: 💤",
                TooltipText = "-",
                ImageId = new ImageId()
            };
        }


        this.output =
            await TimeBasedCache.GetPurityInspectionResultAsync(config.PureInspectorOptions.Prompt, sourceCode);
        //var imageCatalog = new Guid("{AE27A6B0-E345-4288-96DF-5EAF394EE369}");
        //const int imageId = 1851;


        if (output.IsEmpty)
        {
            return new CodeLensDataPointDescriptor
            {
                Description = "λ: 🗸",
                TooltipText = "Empty",
                ImageId = new ImageId()
            };
        }

        // Build a concise description with function names

        var descriptor = new CodeLensDataPointDescriptor
        {
            Description = $"λ: {output.Functions.Length}",
            TooltipText = output.IsEmpty
                ? "-"
                : $"{String.Join(Environment.NewLine,
                    output.Functions.Select(f => $"Name : {f.SuggestedName}:  Description: {f.Description}"))}\r\n\r\n ✦{output.LLmProps}",
            IntValue = 10,
            ImageId = new ImageId(),
        };

        return descriptor;
    }

    public async Task<CodeLensDetailsDescriptor> GetDetailsAsync(
        CodeLensDescriptorContext ctx,
        CancellationToken token
    )
    {
        await Task.CompletedTask.ConfigureAwait(false);

        if (ctx.ApplicableSpan == null)
            return null;

        return null;
    }
}
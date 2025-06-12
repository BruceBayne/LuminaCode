using System;
using System.Threading;
using System.Threading.Tasks;
using AiReview.CodeLens.Vsix.Ai;
using AiReview.CodeLens.Vsix.CodeLens.CodeReview;
using AiReview.Core.LLM;
using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.Review;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;

namespace AiReview.CodeLens.Vsix.CodeLens.BetterNaming;

public class BetterNamingPoint : IAsyncCodeLensDataPoint
{
    private readonly ICodeLensCallbackService devEnv;

    private BetterNamesAnswer output = new();

    private static readonly CodeLensDetailEntryCommand refreshCmdId = new()
    {
        CommandSet = new Guid("2f0c282d-60f6-1a46-1c70-446b61f3be31"),
        CommandId = 0x0100
    };


    public CodeLensDescriptor Descriptor { get; }
    public event AsyncEventHandler InvalidatedAsync;

    public BetterNamingPoint(CodeLensDescriptor descriptor, ICodeLensCallbackService devEnv)
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


        if (!config.BetterNamingOptions.IsEnabled)
        {
            return new CodeLensDataPointDescriptor
            {
                Description = $"💤 BetterNaming: [disabled]",
                TooltipText = "-",
                ImageId = new ImageId(),
            };
        }


        this.output = await TimeBasedCache.GetBetterNamingAsync(config.BetterNamingOptions.Prompt, sourceCode);


        if (output.IsEmpty)
        {
            return new CodeLensDataPointDescriptor
            {
                Description = $"✅ BetterNaming.",
                TooltipText = "-",
                ImageId = new ImageId(),
            };
        }


        var descriptor = new CodeLensDataPointDescriptor
        {
            Description = $"🔠 BetterNaming: {string.Join(", ", output.Names)}",
            TooltipText = $"{output.ToTooltipText()}",
            IntValue = 10,
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
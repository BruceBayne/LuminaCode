using System;
using System.Threading;
using System.Threading.Tasks;
using AiReview.Core.LLM;
using AiReview.Core.LLM.Review;
using AiReview.Core.UI;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;

namespace AiReview.CodeLens.Vsix.CodeLens.CodeReview
{
    public class ReviewDataPoint : IAsyncCodeLensDataPoint
    {
        private readonly ICodeLensCallbackService devEnv;

        private CodeReviewSummary summary = CodeReviewSummary.Dummy;

        private static readonly CodeLensDetailEntryCommand refreshCmdId = new()
        {
            CommandSet = new Guid("1d9c281d-50d6-4276-9c70-374b67f5be52"),
            CommandId = 0x0100
        };


        public CodeLensDescriptor Descriptor { get; }
        public event AsyncEventHandler InvalidatedAsync;

        public ReviewDataPoint(CodeLensDescriptor descriptor, ICodeLensCallbackService devEnv)
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

            var config = await devEnv.InvokeAsync<LuminaCodeProjectOptions>(this, nameof(IAiReviewService.GetProjectOptions),
                [Descriptor.FilePath], token);



            if (!config.ReviewOptions.IsEnabled)
            {
                return new CodeLensDataPointDescriptor
                {
                    Description = $"💤 Review::disabled ",
                    TooltipText = "-",
                    ImageId = new ImageId(),
                };
            }



            summary = await TimeBasedCache.ReviewCodeAsync(config, sourceCode);


            if (summary.IsEmpty || summary.HasOnlyMinorIssues)
            {
                return new CodeLensDataPointDescriptor
                {
                    Description = $"✅ Review:: {summary.LLmProps} ",
                    TooltipText = "-",
                    ImageId = new ImageId(),
                };
            }


            var stars = new string('★', summary.ReviewScore).PadRight(10, '☆');
            var descriptor = new CodeLensDataPointDescriptor
            {
                Description = $"🔍 Review:: {summary.LLmProps} Score: {summary.ReviewScore}/10 {stars}",
                TooltipText = $"{summary} {path} {from} {to}",
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

            var from = ctx.ApplicableSpan.Value.Start;
            var end = ctx.ApplicableSpan.Value.End;


            var sourceCode = await devEnv.InvokeAsync<string>(this, nameof(IAiReviewService.ExtractSourceCode),
                [Descriptor.FilePath, from, end], cancellationToken: token);


            var config = await devEnv.InvokeAsync<LuminaCodeProjectOptions>(this, nameof(IAiReviewService.GetProjectOptions),
                [Descriptor.FilePath], token);


            summary = await TimeBasedCache.ReviewCodeAsync(config, sourceCode);

            return new CodeLensDetailsDescriptor
            {
                Headers =
                [
                    new()
                    {
                        DisplayName = "Tip",
                        IsVisible = true,
                        UniqueName = "Ai-Tip",
                        Width = 1.0,
                    }
                ],
                Entries =
                [
                    new()
                    {
                        Tooltip = "XX",
                        NavigationCommand = refreshCmdId,
                        Fields = [new CodeLensDetailEntryField { Text = "FIELD" }]
                    }
                ],

                CustomData = [new CodeLensDetailsModel { Issues = summary.Issues, ReviewScore = summary.ReviewScore }],

                PaneNavigationCommands =
                [
                    new CodeLensDetailPaneCommand
                    {
                        CommandDisplayName = "Refresh",
                        CommandId = refreshCmdId,
                        CommandArgs = [(object)1]
                    },
                    new CodeLensDetailPaneCommand
                    {
                        CommandDisplayName = "Refresh2",
                        CommandId = refreshCmdId,
                        CommandArgs = [(object)1]
                    }
                ]
            };
        }
    }
}
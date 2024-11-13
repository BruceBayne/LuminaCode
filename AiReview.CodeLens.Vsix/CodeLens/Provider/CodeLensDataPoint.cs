using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AiReview.Core;
using AiReview.Core.OpenAI;
using AiReview.Core.UI;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;

namespace AiReview.CodeLens.Vsix.CodeLens.Provider
{
	public class CodeLensDataPoint : IAsyncCodeLensDataPoint
	{
		private static LmStudioClient aiClient = new();

		private CodeReviewSummary summary  = CodeReviewSummary.Dummy;

		private static readonly CodeLensDetailEntryCommand refreshCmdId = new()
		{
			CommandSet = new Guid("1d9c281d-50d6-4276-9c70-374b67f5be52"),
			CommandId = 0x0100
		};


		public CodeLensDescriptor Descriptor { get; }
		public event AsyncEventHandler InvalidatedAsync;

		public CodeLensDataPoint(CodeLensDescriptor descriptor)
		{
			Descriptor = descriptor;
		}


		async Task<CodeReviewSummary> GenerateAiResponse(string code)
		{
			var chatMessages = new List<ChatMessage>
			{
				new() { role = "user", content = MyWorkspace.AiPromptTemplate },
				new() { role = "user", content = code },
			};

			var aiResponse=await aiClient.GenerateChatResponseAsync(chatMessages).ConfigureAwait(false);
			var reviewSummary = AiResponseConverter.ToReviewSummary(aiResponse);
			return reviewSummary;
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
			var code = MyWorkspace.GetContentFromFile(Descriptor.FilePath, from, to);


			summary = await GenerateAiResponse(code);


			var stars = new string('★', summary.ReviewScore).PadRight(10, '☆');
			
			var descriptor = new CodeLensDataPointDescriptor
			{
				Description = $"AI-Review::{summary.LLmProps} Score: {summary.ReviewScore}/10 {stars}",
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


			var from = ctx.ApplicableSpan.Value.Start;
			var to = ctx.ApplicableSpan.Value.Length;
			var code = MyWorkspace.GetContentFromFile(Descriptor.FilePath, from, to);
			summary = await GenerateAiResponse(code);


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

				CustomData = [new CodeLensDetailsModel(){Issues = summary.Issues, ReviewScore = summary.ReviewScore}],

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
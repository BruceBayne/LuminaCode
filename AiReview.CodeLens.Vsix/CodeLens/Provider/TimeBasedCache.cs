using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiReview.Core;
using AiReview.Core.OpenAI;
using AiReview.Core.UI;

namespace AiReview.CodeLens.Vsix.CodeLens.Provider;

internal sealed class TimeBasedCache
{
	private static readonly LmStudioClient AiClient = new();


	private static readonly ConcurrentDictionary<string, CodeReviewSummary> Cache = new();

	public static async Task<CodeReviewSummary> GenerateAiResponseAsync(string code)
	{
		if (Cache.TryGetValue(code, out var review))
			return review;

		var chatMessages = new List<ChatMessage>
		{
			new() { role = "user", content = PromptDatabase.LostUpdatePrompt },
			new() { role = "user", content = code },
		};

		var aiResponse = await AiClient.GenerateChatResponseAsync(chatMessages).ConfigureAwait(false);
		var reviewSummary = aiResponse.ToReviewSummary().Trim();
		Cache.TryAdd(code, reviewSummary);
		return reviewSummary;
	}
}
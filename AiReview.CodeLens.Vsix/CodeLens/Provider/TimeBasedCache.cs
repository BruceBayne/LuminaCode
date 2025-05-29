using System.Collections.Concurrent;
using System.Threading.Tasks;
using AiReview.Core;
using AiReview.Core.UI;

namespace AiReview.CodeLens.Vsix.CodeLens.Provider;

internal sealed class TimeBasedCache
{
    private static readonly ConcurrentDictionary<string, CodeReviewSummary> Cache = new();


    public static async Task<CodeReviewSummary> GenerateAiResponseAsync(ReviewOptions config, string code)
    {
        var aiPrompt = config.MainPrompt;
        var aiPromptHash = $"{CRC32.Compute(aiPrompt.ToLowerInvariant()):X}";
        var codeHash = $"{CRC32.Compute(code):X}";


        var assistant = ReviewAssistantBuilder
            .Create()
            .WithSystemPrompt(aiPrompt);

        var modelPromptCode = $"{await assistant.GetDefaultModel()}/{aiPromptHash}/{codeHash}";


        if (Cache.TryGetValue(modelPromptCode, out var review))
            return review;

        var aiResponse = await assistant.PerformReview(code);


        Cache.TryAdd(modelPromptCode, aiResponse);
        return aiResponse;
    }
}
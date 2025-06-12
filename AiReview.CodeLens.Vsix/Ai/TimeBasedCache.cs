using System.Collections.Concurrent;
using System.Threading.Tasks;
using AiReview.Core;
using AiReview.Core.LLM;
using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.Review;
using AiReview.Core.OpenAI.Client;

namespace AiReview.CodeLens.Vsix.Ai;

internal sealed class TimeBasedCache
{
    private static readonly ConcurrentDictionary<string, CodeReviewSummary> ReviewCache = new();
    private static readonly ConcurrentDictionary<string, BetterNamesAnswer> BetterNamingCache = new();


    public static async Task<BetterNamesAnswer> GetBetterNamingAsync(string prompt, string code)
    {
        var aiPromptHash = $"{CRC32.Compute(prompt.ToLowerInvariant()):X}";
        var codeHash = $"{CRC32.Compute(code):X}";


        var assistant = OpenAiAssistantBuilder
            .Create()
            .WithSystemPromptRaw(prompt);

        var modelPromptCode = $"{await assistant.GetDefaultModel()}/{aiPromptHash}/{codeHash}";

        if (BetterNamingCache.TryGetValue(modelPromptCode, out var betterNames))
            return betterNames;

        var aiResponse = await assistant.GetBetterNames(code);
        BetterNamingCache.TryAdd(modelPromptCode, aiResponse);
        return aiResponse;
    }


    public static async Task<CodeReviewSummary> ReviewCodeAsync(LuminaCodeProjectOptions config, string code)
    {
        var aiPrompt = config.ReviewOptions.Prompt;
        var aiPromptHash = $"{CRC32.Compute(aiPrompt.ToLowerInvariant()):X}";
        var codeHash = $"{CRC32.Compute(code):X}";


        var assistant = OpenAiAssistantBuilder
            .Create()
            .WithSystemPrompt(aiPrompt);

        var modelPromptCode = $"{await assistant.GetDefaultModel()}/{aiPromptHash}/{codeHash}";


        if (ReviewCache.TryGetValue(modelPromptCode, out var review))
            return review;

        var aiResponse = await assistant.PerformReview(code);


        ReviewCache.TryAdd(modelPromptCode, aiResponse);
        return aiResponse;
    }
}
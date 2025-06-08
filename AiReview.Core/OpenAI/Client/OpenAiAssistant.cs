using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.Review;

namespace AiReview.Core.OpenAI.Client;

public sealed class OpenAiAssistant
{
    private OpenAIClient lmClient = new();

    public IEnumerable<ChatMessage> systemPrompt { get; set; }


    /// <summary>
    /// Performs a code review using fast estimation if it meets the score threshold; otherwise, uses detailed review.
    /// </summary>
    /// <param name="userPrompt">The prompt describing the code.</param>
    /// <param name="scoreGate">Threshold for fast estimation (default: 5).</param>
    /// <param name="temperature">Controls result variability (default: 0).</param>
    /// <returns>A summary of the code review.</returns>
    public async Task<CodeReviewSummary> PerformReviewFast
    (
        string userPrompt,
        int scoreGate = 5,
        float temperature = 0
    )
    {
        var estimation = await EstimateCodeRating(userPrompt, temperature);
        if (estimation != null && estimation.Rating > scoreGate)
            return CodeReviewSummary.FromFastEstimation(userPrompt, temperature, estimation);

        return await PerformReview(userPrompt, temperature);
    }


    public async Task<string> GetDefaultModel()
    {
        var allModels = await lmClient.GetModelsAsync();

        if (allModels == null)
            throw new Exception("No models loaded");

        var model = allModels.Data.First().Id;
        return AiResponseConverter.TrimModel(model);
    }


    public async Task<BetterNamesAnswer> GetBetterNames(string userPrompt, float temperature = 0)
    {
        var chatMessages = ToChatMessages(userPrompt);
        var response = await lmClient.GenerateChatResponseAsync(chatMessages, temperature: temperature);
        var summary = response.ToBetterNames();
        summary.Temperature = temperature;
        summary.RawRequest = string.Join(Environment.NewLine, chatMessages.Select(x => x.content));
        summary.Duration = response.Duration;
        return summary;
    }

    /// <summary>
    /// Performs a detailed code review based on the user prompt and returns a summary.
    /// </summary>
    /// <param name="userPrompt">The prompt describing the code.</param>
    /// <param name="temperature">Controls result variability (default: 0).</param>
    /// <returns>A summary of the detailed code review.</returns>
    public async Task<CodeReviewSummary> PerformReview(string userPrompt, float temperature = 0)
    {
        var chatMessages = ToChatMessages(userPrompt);
        var response = await lmClient.GenerateChatResponseAsync(chatMessages, temperature: temperature);
        var summary = response.ToReviewSummary();
        summary.Temperature = temperature;
        summary.Duration = response.Duration;
        summary.RawRequest = string.Join(Environment.NewLine, chatMessages.Select(x => x.content));
        return summary;
    }

    private IReadOnlyCollection<ChatMessage> ToChatMessages(string userPrompt)
    {
        var systemPromptText = string.Join($"{Environment.NewLine}", systemPrompt.Select(x => x.content));

        var chatMessages = new List<ChatMessage>
        {
            new()
            {
                role = "user",
                content = $"{systemPromptText}{Environment.NewLine}{userPrompt}",
            }
        };
        return chatMessages;
    }


    public record CodeEstimation
    {
        public string Model { get; set; }
        public int Rating { get; set; }

        public string RawResponse { get; set; }
    }

    public async Task<CodeEstimation> EstimateCodeRating(string userPrompt, float temperature = 0)
    {
        var chatMessages = ToChatMessages(userPrompt);

        var tokensAfterRating = 7;

        var builder = new StringBuilder();
        var model = await GetDefaultModel();
        await lmClient.GenerateChatResponseWithStreamingAsync(chunk =>
        {
            var contentToken = chunk.Choices?.FirstOrDefault()?.Delta?.Content;

            model ??= chunk.Model;

            if (!string.IsNullOrEmpty(contentToken))
            {
                builder.Append(contentToken);
                if (builder.ToString().Contains("rank"))
                    tokensAfterRating--;
            }

            return tokensAfterRating > 0;
        }, messages: chatMessages, temperature);


        var rating = ExtractRating(builder.ToString());
        if (rating.HasValue)
        {
            return new CodeEstimation
            {
                Model = model,
                Rating = rating.Value,
                RawResponse = builder.ToString(),
            };
        }

        return null;
    }

    private static int? ExtractRating(string input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        // Define a regex pattern to find "rating": [value]
        string pattern = @"""rank""\s*:\s*(\d+)";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

        if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
        {
            return value;
        }

        return null; // Return null if no match or parse fails
    }
}
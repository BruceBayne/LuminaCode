using System;

namespace AiReview.Core.LLM.Naming;

[Serializable]
public class BetterNamingOptions
{
    public bool IsEnabled { get; set; } = true;
    public string Prompt { get; set; } = PromptDatabase.BetterNamingPrompt;
}
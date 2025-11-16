using System;

namespace AiReview.Core.LLM.PurityInspector;

[Serializable]
public class PurityInspectorOptions
{
    public bool IsEnabled { get; set; } = true;
    public string Prompt { get; set; } = PromptDatabase.PurityInspectionPrompt;
}
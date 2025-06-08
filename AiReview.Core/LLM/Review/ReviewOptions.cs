using System;

namespace AiReview.Core.LLM.Review
{
    

    [Serializable]
    public class ReviewOptions
    {
        public bool IsEnabled { get; set; } = true;
        public string Prompt { get; set; } = PromptDatabase.GeneralReviewPrompt;
    }
}
using System;

namespace AiReview.Core.LLM.Naming
{
    [Serializable]
    public class BetterNamesAnswer : BasicAnswer
    {
        public string[] Names { get; set; }

        public bool IsEmpty => Names == null || Names.Length == 0;

        public static BetterNamesAnswer Empty() => new() { Names = [] };

        public override string ToString() => IsEmpty ? "No names available." : string.Join(",", Names);

        public string ToTooltipText() => $"TokensPerSecond:{TokensPerSecond:F2}";
    }
}
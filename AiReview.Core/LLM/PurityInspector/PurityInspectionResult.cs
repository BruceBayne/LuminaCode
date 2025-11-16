using System;
using System.Linq;

namespace AiReview.Core.LLM.PurityInspector
{
    [Serializable]
    public sealed class PurityInspectionResult : BasicAnswer
    {
        public FunctionInfo[] Functions { get; set; } = Array.Empty<FunctionInfo>();

        public bool IsEmpty => Functions.Length <= 1;

        public static PurityInspectionResult Empty() =>
            new() { Functions = Array.Empty<FunctionInfo>() };

        public override string ToString() =>
            IsEmpty
                ? ""
                : string.Join(", ", Functions.Select(f => f.SuggestedName));

        public string ToTooltipText() =>
            $"TokensPerSecond:{TokensPerSecond:F2} ✦{this.LLmProps} ";
    }

    public sealed record FunctionInfo
    {
        public string SuggestedName { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
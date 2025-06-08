using AiReview.Core.LLM;

namespace AiReview.CodeLens.Vsix.CodeLens;

interface IAiReviewService
{
    string ExtractSourceCode(string filePath, int from, int to);
    LuminaCodeProjectOptions GetProjectOptions(string filePath);
}
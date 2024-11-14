namespace AiReview.CodeLens.Vsix.CodeLens;

interface IAiReviewService
{
	string ExtractSourceCode(string filePath, int from,int to);


}
using AiReview.Core.LLM.Review;
using AiReview.Core.UI;

namespace LuminaCode_cli;

public sealed class ReviewLiveStats
{
	private sealed class SymbolsPerSecond
	{
		private readonly DateTime dt = DateTime.Now;
		public float SymbolsPs { get; private set; }
		private int totalLen;

		public void Update(CodeReviewSummary summary)
		{
			if (!string.IsNullOrEmpty(summary.RawResponse) && summary.Issues.Any())
			{
				var duration = (DateTime.Now - dt);
				totalLen += summary.RawResponse.Length;
				SymbolsPs = (float)(totalLen / duration.TotalSeconds);
			}
		}
	}

	public int TotalEntities;
	public string AiModel = "";
	private int processedEntities;
	private readonly DateTime startTime = DateTime.Now;
	private readonly SymbolsPerSecond sps = new();

	private void Update(bool finalReviewDone = false)
	{
		var elapsed = DateTime.Now - startTime;
		
		if (elapsed.TotalSeconds >= 1)
		{
			var averageEntitiesPerSecond = processedEntities / elapsed.TotalSeconds;
			var perMinute = averageEntitiesPerSecond * 60;
			var stats = $"LuminaCode-cli | Uptime: {elapsed:g} | {AiModel} | Processed&Total Entities : {processedEntities}/{TotalEntities} | Symbols p/s : {sps.SymbolsPs:F1} | Average Speed: {averageEntitiesPerSecond:F2} entities/sec  {perMinute:F2} entities/min";
			Console.Title = stats;

			if (finalReviewDone)
			{
				Console.WriteLine($"[Completed] / {stats}");
			}
			else if (processedEntities % 10 == 0)
			{
				Console.WriteLine($"{Environment.NewLine} -------- {stats} ---------- {Environment.NewLine}");
			}
		}
	}

	public void SingleElementReviewed(CodeReviewSummary summary)
	{
		processedEntities++;
		sps.Update(summary);
		Update();
	}

	public void Completed()
	{
		Update(finalReviewDone: true);
	}
}
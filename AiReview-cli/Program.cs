using CommandLine;


namespace LuminaCode_cli
{
	public sealed class Program
	{
		public static async Task Main(string[] args)
		{
			Console.WriteLine("LuminaCode-cli 0.1-preview.");

			await Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(HandleParseError)
				.WithParsedAsync(HandleParserOk);
		}


		private static async Task HandleParserOk(Options opt)
		{
			var engine = new LuminaCodeCliEngine(opt);
			var liveStats = new ReviewLiveStats();

			var context = await engine.CreateContext();
			liveStats.TotalEntities = context.ToReview.Count;
			liveStats.AiModel = context.AiModel;

			await engine.PerformReview(context,(currentEntity, summary) =>
			{
				liveStats.SingleElementReviewed(summary);
			});

			liveStats.Completed();
		}

		private static void HandleParseError(IEnumerable<Error> obj)
		{
		}
	}
}
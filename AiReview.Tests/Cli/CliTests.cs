using AiReview.Tests.Model;
using LuminaCode_cli;
using CommandLine;
using FluentAssertions;

namespace AiReview.Tests.Cli
{

	[Collection("cli")]
	public sealed class CliTests
	{
		private readonly string sourcePath = Path.Combine(TestCases.GetTestCasesFolder());

		[Fact]
		public void ConsoleParserWorks()
		{
			var p = Parser.Default.ParseArguments<Options>(["-i", sourcePath, "-v", "-byPassAiChecks", "no-db-update"]);
			p.Errors.Should().BeEmpty();
			p.Value.Should().NotBeNull();
		}

		[Fact]
		public async Task EmbeddedCasesWorks()
		{
			var options = new Options
			{
				BypassAiChecks = true,
				ScanFolderPath = sourcePath,
				Verbose = true,
				DatabaseInputPath = "z:\\in.json",
				//DatabaseOutputPath = "z:\\out.json",
				AiFastEstimationEnabled = true,
				ReviewNewItemsOnly = true,
				ImmediateCommit = true,
			};

			var engine = new LuminaCodeCliEngine(options);

			var context = await engine.CreateContext();
			context.ToReview.Should().HaveCountGreaterOrEqualTo(10);
			
			//options.ReviewNewItemsOnly = true;
			//engine = new LuminaCodeCliEngine(options);
			//await engine.PerformReview();
		}
	}
}
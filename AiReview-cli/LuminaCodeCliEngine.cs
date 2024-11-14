using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using AiReview.Core;
using AiReview.Core.Database;
using AiReview.Core.UI;
using Colorful;
using LuminaCode_cli.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;

namespace LuminaCode_cli;

public sealed class LuminaCodeCliEngine
{
	private readonly Options options;
	private readonly string AiPrompt;
	private readonly string AiPromptHash;

	private ReviewDatabase? inputDatabase;
	private ReviewDatabase? outputDatabase;

	public LuminaCodeCliEngine(Options options)
	{
		this.options = options;
		System.Console.OutputEncoding = System.Text.Encoding.UTF8;

		Colorful.Console.WriteLine(" ** LuminaCode-cli-engine ** ", Color.LawnGreen);


		LogV($"[{nameof(options.ScanFolderPath)}] : {options.ScanFolderPath}");

	

		AiPrompt = GetReviewPrompt();
		AiPromptHash = $"{CRC32.Compute(AiPrompt.ToLowerInvariant()):X}";

		if (string.IsNullOrEmpty(options.DatabaseInputPath))
		{
			options.DatabaseInputPath = Path.Combine(options.ScanFolderPath, "luminaCode.db.json");
			LogV($"[{nameof(options.DatabaseInputPath)}]: {options.DatabaseInputPath}");
		}

		if (string.IsNullOrEmpty(options.DatabaseOutputPath))
		{
			options.DatabaseOutputPath = options.DatabaseInputPath;
		}


		LogV($"[{nameof(options.DatabaseOutputPath)}]: {options.DatabaseOutputPath}");
		LogV($"[AI-Fast Estimation]: {options.AiFastEstimationEnabled}");
		LogV($"[AI-PromptHash]: {AiPromptHash} [Length]: {AiPrompt.Length}");


		inputDatabase = SolutionDatabase.CreateOrLoadDatabase(options.DatabaseInputPath);


		if (options.DatabaseInputPath == options.DatabaseOutputPath)
		{
			outputDatabase = inputDatabase;
		}
		else
		{
			outputDatabase = new ReviewDatabase();
		}
	}

	private string GetReviewPrompt()
	{

		var aiPrompt = PromptDatabase.GeneralReviewPrompt;
		var promptPath = options.AiPromptPath;

		if (!string.IsNullOrEmpty(promptPath))
		{
			if (!Path.IsPathRooted(promptPath))
			{
				promptPath = Path.Combine(options.ScanFolderPath, options.AiPromptPath);
			}

			if (File.Exists(promptPath))
			{
				LogV($"[{nameof(options.AiPromptPath)}]: {promptPath}");
				aiPrompt = File.ReadAllText(promptPath);
			}
			else
			{
				throw new Exception("File not found: " + promptPath);
			}
		}

		return aiPrompt.Trim();
	}

	private void LogV(string s)
	{
		if (options.Verbose)
		{
			var styleSheet = new StyleSheet(Color.White);
			styleSheet.AddStyle("\\[([^\\]]+)\\]", Color.Yellow);
			styleSheet.AddStyle("[ ]\\d+", Color.Lime);
			styleSheet.AddStyle("^.*Skipping.*$", Color.Gray);
			styleSheet.AddStyle("^\\d\\d:\\d\\d:\\d\\d", Color.DarkGray);
			styleSheet.AddStyle("Review done in", Color.DarkGray);
			Colorful.Console.WriteLineStyled($@"{DateTime.Now:HH:mm:ss} | {s}", styleSheet);
		}
	}


	private static void Log(string s)
	{
		var styleSheet = new StyleSheet(Color.White);
		styleSheet.AddStyle("\\[([^\\]]+)\\]", Color.Yellow);
		styleSheet.AddStyle("[ ]\\d+", Color.Lime);
		styleSheet.AddStyle("^.*Skipping.*$", Color.Gray);
		styleSheet.AddStyle("^\\d\\d:\\d\\d:\\d\\d", Color.DarkGray);
		styleSheet.AddStyle("Review done in", Color.DarkGray);
		Colorful.Console.WriteLineStyled($@"{DateTime.Now:HH:mm:ss} | {s}", styleSheet);
	}


	public sealed class SolutionContext
	{
		public IReadOnlyCollection<MethodData> ToReview { get; init; }
		public string AiModel { get; init; }
		public ReviewAssistant assistant { get; init; }
	}


	public async Task<SolutionContext> CreateContext()
	{
		
		var assistant = ReviewAssistantBuilder
			.Create()
			.WithSystemPrompt(AiPrompt);

		var aiModel = GetModelName(options.BypassAiChecks ? "empty" : await assistant.GetDefaultModel());
		LogV($"Selected AI-model: {aiModel}");

		var context = new SolutionContext
		{
			AiModel = aiModel, 
			assistant = assistant, 
			ToReview = GetThingsToReview(aiModel)
		};

		return context;

	}




	public async Task PerformReview(SolutionContext context, Action<MethodData, CodeReviewSummary>? callback = null)
	{
		Log($"{nameof(LuminaCodeCliEngine)}::{nameof(PerformReview)} ->");
		
		foreach (var md in context.ToReview)
		{
			try
			{
				var summary = CodeReviewSummary.Empty();
				var identity = $"[{md.ToHash()}]/[{md.NavPath}]";

				if (!options.BypassAiChecks)
				{
					var stopwatch = Stopwatch.StartNew();
					if (options.AiFastEstimationEnabled)
					{
						Log($"⏩ | Fast-Reviewing {identity}");
						summary = await context.assistant.PerformReviewFast(md.ToAnalyze);
					}
					else
					{
						Log($"⏳ | Reviewing {identity}");
						summary = await context.assistant.PerformReview(md.ToAnalyze);
					}

					stopwatch.Stop();
					var elapsedTime = stopwatch.Elapsed.ToString(@"mm\:ss");
					LogV($"✅ | Review done in: {elapsedTime} with {nameof(summary.ReviewScore)}: {summary.ReviewScore}");
					summary.LLmProps = GetModelName(summary.LLmProps);
				}
				else
				{
					summary.LLmProps = context.AiModel;
					LogV($"❌ | Bypassing review {identity}");
				}


				callback?.Invoke(md, summary);

				if (Append(md, summary))
				{
					if (options.ImmediateCommit)
						await CommitOutputDatabase();
				}
			}
			catch (Exception e)
			{
				Log($"❗ | We got exception while reviewing : {e.Message}");
			}
		}

		await CommitOutputDatabase();
	}

	private MethodData[] GetThingsToReview(string aiModel)
	{
		var scoresByNavPath = inputDatabase
			.Elements
			.GroupBy(sc => sc.Key)
			.ToDictionary(
				g => g.Key,
				g => g.First().Value.Reports.Min(x => x.ReviewScore)
			);

		var methodsClasses = GetEntitiesToAnalyze()
			.Where(e => IsReadyToAnalyze(aiModel, e))
			.OrderBy(fc => scoresByNavPath.GetValueOrDefault(fc.ToHash(), int.MaxValue))
			.ToArray();
		return methodsClasses;
	}


	private static readonly Regex TestPattern = new(@"^(?i).*(Test|Tests|Spec|UnitTest).*$");

	public static bool IsTestClassOrPath(string nameOrPath) => TestPattern.IsMatch(nameOrPath);


	public static bool IsPartialClass(ClassDeclarationSyntax? classDeclaration)
	{
		// Check if the Modifiers collection contains the 'partial' keyword
		return classDeclaration is not null &&
		       classDeclaration.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword));
	}


	private bool IsReadyToAnalyze(string aiModel, MethodData arg)
	{
		// Define a HashSet with the items you want to check
		var excludedKeywords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			"Generated",
			".g.",
			".xaml."
		};

		// Check if the NavPath contains any of the excluded keywords
		if (excludedKeywords.Any(keyword => arg.NavPath.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)))
		{
			LogV($"🚫 | Skipping Analysis of {arg.NavPath} due to excluded content");
			return false;
		}


		if (IsPartialClass(arg.ClassDeclaration))
		{
			LogV($"🚫 | Skipping Analysis of {arg.NavPath} due to partial class content");
			return false;
		}


		if (IsTestClassOrPath(arg.SourceFilePath))
		{
			LogV($"🚫 | Skipping Analysis of {arg.NavPath} due to test case.");
			return false;
		}


		if (arg.EntityType == "method" && options.SkipMethods)
		{
			LogV($"🚫 | Skipping Method Analysis of {arg.NavPath}.");
			return false;
		}


		if (options.ReviewNewItemsOnly && inputDatabase is { Elements: not null })
		{
			var hash = arg.ToHash();
			if (!inputDatabase.Elements.TryGetValue(hash, out var reviewAggregate))
				return true;

			var analysisDoneBefore = reviewAggregate.Reports.Any(x => x.Model == aiModel);


			if (analysisDoneBefore)
			{
				LogV($"⚪ | {arg.NavPath} - Analysis already conducted by {aiModel}. Skipping.");
			}

			return !analysisDoneBefore;
		}

		return true;
	}

	public MethodData[] GetEntitiesToAnalyze()
	{
		var methodsClasses = MethodScanner.Scan(options.ScanFolderPath).ToArray();
		Log($"📊 | Entities to review: {methodsClasses.Length}");
		return methodsClasses;
	}


	private async Task CommitOutputDatabase()
	{
		if (outputDatabase != null)
		{
			foreach (var record in outputDatabase.Elements.ToArray())
			{
				var ordered = record.Value.Reports.OrderBy(x => x.ReviewScore).ToList();
				outputDatabase.Elements[record.Key].Reports = ordered;
			}


			outputDatabase.Elements = outputDatabase.Elements.OrderBy(x => x.Value.Reports.Min(x => x.ReviewScore))
				.ToDictionary();


			var content = JsonConvert.SerializeObject(outputDatabase, SolutionDatabase.SerializerSettings);
			await File.WriteAllTextAsync(options.DatabaseOutputPath, content);
		}
	}

	private bool Append(MethodData mi, CodeReviewSummary summary)
	{
		if (outputDatabase != null)
		{
			var model = GetOrCreate(mi, summary);

			if (model is not null)
			{
				model.Model = summary.LLmProps;
				model.Issues = summary.Issues;
				model.ReviewScore = summary.ReviewScore;
				return true;
			}
		}

		return false;
	}

	private string GetModelName(string modelName) => $"{modelName}/{AiPromptHash}";

	private ReviewReport? GetOrCreate(MethodData mi, CodeReviewSummary summary)
	{
		var hash = mi.ToHash();
		var modelName = summary.LLmProps;


		var aggregateExists = outputDatabase!.Elements.TryGetValue(hash, out var reviewAggregate);


		if (options.ReviewNewItemsOnly && inputDatabase != default &&
		    inputDatabase.Elements.TryGetValue(hash, out var referenceList))
		{
			var reviewExistsInReference = referenceList.Reports.Any(x => x.Model == modelName);

			if (reviewExistsInReference)
			{
				return default;
			}
		}


		if (!aggregateExists)
		{
			reviewAggregate = new ReviewAggregate
			{
				Reports = []
			};
			outputDatabase.Elements.Add(hash, reviewAggregate);
		}

		reviewAggregate!.NavPath = mi.NavPath;
		var reviewReport = reviewAggregate.Reports.FirstOrDefault(x => x.Model == modelName);

		if (reviewReport is null)
		{
			reviewReport = new ReviewReport { Model = modelName };
			reviewAggregate.Reports.Add(reviewReport);
		}

		return reviewReport;
	}
}
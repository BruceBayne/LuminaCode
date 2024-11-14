using CommandLine;

namespace LuminaCode_cli;

public class Options
{
	[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
	public bool Verbose { get; set; }


	[Option('c', "immediate-commit", Required = false, HelpText = "Commit immediately", Default = true)]
	public bool ImmediateCommit { get; set; }



	[Option('b', "ai-bypass-checks", Required = false, HelpText = "Bypass AI checks")]
	public bool BypassAiChecks { get; set; }


	//[Option("skip-db-update", Required = false, HelpText = "Do not write to internal db")]
	//public bool SkipDbUpdate { get; set; }


	[Option('e', "ai-fast-estimation", Required = false,
		HelpText = "Enable fast estimation algorithm interruption to avoid fetching complete LLM results, potentially improving speed on slower models.", 
		Default = false)]
	public bool AiFastEstimationEnabled { get; set; }

	[Option('p', "ai-prompt", Required = false, HelpText = "AI-Prompt file path")]
	public string AiPromptPath { get; set; }


	[Option('i', "scan-folder-path", Required = true, HelpText = "Project/Solution sources folder path")]
	public string ScanFolderPath { get; set; }

	[Option('o', "db-output-file", Required = false, HelpText = "Database output file path")]
	public string DatabaseOutputPath { get; set; }

	
	[Option('r', "db-input-file", Required = false, HelpText = "Database input file path")]
	public string DatabaseInputPath { get; set; }


	[Option('n', "review-new-items-only", Required = false, HelpText = "Review only items which are not in reference(input) database-file", Default = true)]
	public bool ReviewNewItemsOnly { get; set; }


	[Option('m', "skip-methods", Required = false, HelpText = "Skip methods scanning.", Default = true)]
	public bool SkipMethods { get; set; }


	//[Value(0, MetaName = "offset", HelpText = "File offset.")]
	//public long? Offset { get; set; }

}
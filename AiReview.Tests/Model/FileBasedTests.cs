using AiReview.Core.LLM;
using AiReview.Core.LLM.Review;
using AiReview.Core.OpenAI;
using AiReview.Core.OpenAI.Client;
using FluentAssertions;
using Xunit.Abstractions;

namespace AiReview.Tests.Model;

[Collection("LM-Studio")]
public sealed class FileBasedTests(ITestOutputHelper output) : BasePromptTest
{
    private void DumpResponse(DateTime dt, CodeReviewSummary summary)
    {
        float symbolsPs = 0;
        var duration = (DateTime.Now - dt);

        if (!string.IsNullOrEmpty(summary.RawResponse))
        {
            symbolsPs = (float)(summary.RawResponse.Length / duration.TotalSeconds);
        }


        output.WriteLine(
            $"LLM : {summary.LLmProps} / Temperature : {summary.Temperature} Symbols : {symbolsPs:F1} p/s Duration: {duration:g}");

        output.WriteLine($"{Environment.NewLine}[Code Review Summary] Length:{summary.RawResponse.Length}");
        output.WriteLine(summary.ToString());


        const string separator = "----------------------------";
        output.WriteLine(separator);
        output.WriteLine($"{Environment.NewLine}{Environment.NewLine}> [RAW LLM RESPONSE]");
        output.WriteLine(summary.RawResponse);

        output.WriteLine(separator);
        output.WriteLine($"{Environment.NewLine}{Environment.NewLine}> [RAW LLM REQUEST]");
        output.WriteLine(summary.RawRequest);
    }


    [Fact]
    public async Task Foo()
    {
        //File.WriteAllText("z:\\1.json", JsonConvert.SerializeObject(LuminaCodeProjectOptions.Default,Formatting.Indented));

        string aiPrompt = PromptDatabase.BetterNamingPrompt;

        var assistant = OpenAiAssistantBuilder
            .Create()
            .WithSystemPromptRaw(aiPrompt);

        var prompts = new[]
        {
            """
            public bool ZZ(int x) => x%2 !=0;
            """,

            """
            pulic class FooBar
            {
                public int id { get; set; }
                public string Name { get; set; }
                public string BtcWalletId { get; set; }
            }
            """,
            "\r\n\t[Fact]\r\n\tpublic async Task<string> Foo(int people, int salary) {await Task.CompletedTask; return 1/0;}",
            "class Zuu{ public string Name { get; set; } public int Age { get; set; } }",
            "IEnumerable<int> ComputeFoo(int a, int b) => Enumerable.Range(0, a).SelectMany(x => Enumerable.Range(0, b), (x, y) => x * b + y);\n",
            "bool GetZalupa(string x) => !System.Text.RegularExpressions.Regex.IsMatch(x, @\"^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$\");"
        };

        foreach (var prompt in prompts)
        {
            var output = await assistant.GetBetterNames(prompt);


            //var resp = await expClient.SendMessageAsync(aiPrompt, prompt);
            //var text = string.Join(",", resp.Messages.Select(x => x.Content));
            //Console.WriteLine(text);

            output.Names.Should().NotBeNull();
            Console.WriteLine(output.RawRequest);
            Console.WriteLine($"TPS: {output.LLmProps}/{output.TokensPerSecond} Output: {output}");
            Console.WriteLine("-----");
        }
    }


    [Theory(Timeout = 45000)]
    [ClassData(typeof(TestCases))]
    [Trait("Category", "File-Based")]
    public async Task VerifyCase(string fileName, string testName)
    {
        output.WriteLine($"TestCase: {testName}");


        var snippet = await ExtractSnippet(fileName, testName);
        var reviewPrompt = GetReviewPrompt(snippet);

        var assistant = OpenAiAssistantBuilder
            .Create()
            .WithSystemPrompt(reviewPrompt);


        var dt = DateTime.Now;
        var summary = await assistant.PerformReview(snippet.SourceCode, temperature: snippet.Temperature);
        DumpResponse(dt, summary);


        output.WriteLine("\r\n\r\n\r\n");

        await ValidateSnippetReview(snippet, summary);
    }

    private string GetReviewPrompt(TestSnippet snippet)
    {
        if (string.Compare(snippet.ReviewPrompt, "general", StringComparison.OrdinalIgnoreCase) == 0)
        {
            output.WriteLine($"{nameof(PromptDatabase.GeneralReviewPrompt)} chosen.");
            return PromptDatabase.GeneralReviewPrompt;
        }

        if (string.Compare(snippet.ReviewPrompt, "database", StringComparison.OrdinalIgnoreCase) == 0)
        {
            output.WriteLine($"{nameof(PromptDatabase.LostUpdatePrompt)} chosen.");
            return PromptDatabase.LostUpdatePrompt;
        }

        return PromptDatabase.GeneralReviewPrompt;
    }


    private async Task ValidateSnippetReview(TestSnippet snippet, CodeReviewSummary summary)
    {
        await Task.CompletedTask;

        if (snippet.HumanReview.Any())
        {
            var lmc = new OpenAIClient();

            foreach (var manualFinding in snippet.HumanReview)
            {
                ValidateHumanReviewToAiReview(manualFinding, summary);


                //var similarity = new List<float>();
                //foreach (var issue in summary.Issues)
                //{
                //	var toAnalyze=issue.Description.Split(".", StringSplitOptions.RemoveEmptyEntries);

                //	foreach (var llmFinding in toAnalyze)
                //	{
                //		similarity.Add(await lmc.ComputeSimilarity(manualFinding, llmFinding));
                //	}

                //}

                //var topMatch = similarity.OrderByDescending(x=>x).First();

                //output.WriteLine($"Top similarity found for '{manualFinding}' with Best-Score: {topMatch}");

                //if (topMatch < 0.8f)
                //{
                //	throw new Exception();
                //}
            }
        }

        summary.ReviewScore.Should().BeInRange(0, 10);
        summary.Issues.Should().NotBeNull();
        summary.LLmProps.Should().NotBeEmpty();
        summary.RawResponse.Should().NotBeNullOrEmpty();
        summary.RawRequest.Should().NotBeNullOrEmpty();
        summary.RawRequest.Should().NotBeNullOrEmpty();


        if (summary.Issues.Any())
        {
            summary.ReviewScore.Should().BeInRange(snippet.MinRating, snippet.MaxRating);
        }
    }

    private static void ValidateHumanReviewToAiReview(string humanReview, CodeReviewSummary summary)
    {
        const string notPattern = "~NO~ ";

        var hasInversion = humanReview.StartsWith(notPattern);

        humanReview = humanReview.Replace(notPattern, string.Empty);
        var finding =
            summary.Issues.FirstOrDefault(x => x.Description.Contains(humanReview, StringComparison.OrdinalIgnoreCase));

        if (hasInversion)
        {
            finding.Should()
                .BeNull(because: $"NO \"{humanReview}\" should be available.");
        }
        else
        {
            finding.Should().NotBeNull(because: $"Expected to find \"{humanReview}\" in summary.");
        }
    }
}
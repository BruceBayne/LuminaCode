using AiReview.Core;
using AiReview.Core.OpenAI;
using AiReview.Core.UI;
using System.Text.RegularExpressions;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AiReview.Tests;

[Collection("LM-Studio")]
public sealed class LmStudioTests(ITestOutputHelper testOutputHelper)
{
	

	public CodeLensDetailsModel ToModel(string llmOutput)
	{
		return null;
	}


	[Theory, CombinatorialData]
	[Trait("Category", "Simple")]
	public async Task SimpleMain([CombinatorialValues(0, 0.5, 0.9, 5)] float temperature) =>
		await DumpResponse(MyWorkspace.AiPromptTemplate, CodeCollections.MinSample, temperature: temperature);


	[Theory, CombinatorialData]
	[Trait("Category", "Race-Condition")]
	public async Task RaceCondition([CombinatorialValues(0, 0.5, 0.9, 5)] float temperature) =>
		await DumpResponse(MyWorkspace.AiPromptTemplate, CodeCollections.RaceConditionSample, temperature: temperature);


	[Theory, CombinatorialData]
	[Trait("Category", "Race-Condition")]
	public async Task EFFullClass_RaceCondition([CombinatorialValues(0, 0.5, 0.9, 5)] float temperature)
	{
		await DumpResponse(MyWorkspace.AiPromptTemplate, CodeCollections.ClassBased_EFLostUpdate,
			temperature: temperature);
		
	}

	[Theory, CombinatorialData]
	[Trait("Category", "Race-Condition")]
	public async Task EFLostUpdate([CombinatorialValues(0, 0.5, 0.9, 5)] float temperature) =>
		await DumpResponse(MyWorkspace.AiPromptTemplate,CodeCollections.EFLostUpdate, temperature: temperature);


	[Theory, CombinatorialData]
	[Trait("Category", "Simple")]
	public async Task ChatGptGeneratedPoorCode([CombinatorialValues(0, 0.5, 0.9, 5)] float temperature) =>
		await DumpResponse(MyWorkspace.AiPromptTemplate, CodeCollections.ChatGptGeneratedPoorCode, temperature: temperature);



	private async Task DumpResponse(string systemPrompt, string codeInput, float temperature)
	{
		var lmClient = new LmStudioClient();


		var chatMessages = new List<ChatMessage>
		{
			new() { role = "user", content = systemPrompt },
			new() { role = "user", content = codeInput },
		};


		var response = await lmClient.GenerateChatResponseAsync(chatMessages, 500, temperature);
		if (response != null)
		{
			testOutputHelper.WriteLine($"LLM : {response.Model} / Temperature : {temperature}");

		

		
			var aiResponse = response.Choices.First().Message.content;



			var reviewSummary = AiResponseConverter.ToReviewSummary(response);
			

			testOutputHelper.WriteLine($"{Environment.NewLine}[Code Review Summary]");
			testOutputHelper.WriteLine(reviewSummary.ToString());
			reviewSummary.ReviewScore.Should().BeInRange(0, 10);





			testOutputHelper.WriteLine("----------------------------");
			testOutputHelper.WriteLine($"{Environment.NewLine}{Environment.NewLine}> [RAW LLM RESPONSE]");
			testOutputHelper.WriteLine(aiResponse);





			testOutputHelper.WriteLine("----------------------------");
			testOutputHelper.WriteLine($"{Environment.NewLine}{Environment.NewLine}> [FULL LLM REQUEST]");
			foreach (var message in chatMessages)
			{
				testOutputHelper.WriteLine(message.content);
			}


		}
	}
}
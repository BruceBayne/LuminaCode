using System.Collections.Generic;
using AiReview.Core.LLM;

namespace AiReview.Core.OpenAI.Client;

public static class OpenAiAssistantBuilder
{
	public static OpenAiAssistant Create()
	{
		return new OpenAiAssistant();
	}

	public static OpenAiAssistant WithSystemPrompt(this OpenAiAssistant ra, string systemPrompt)
	{
		var chatMessages = new List<ChatMessage>
		{
			new(role: "System", content: systemPrompt),
			new(role: "System", content: PromptDatabase.ReviewOutputPrompt),
		};

		ra.systemPrompt = chatMessages;
		return ra;
	}

	public static OpenAiAssistant WithSystemPromptRaw(this OpenAiAssistant ra, string systemPrompt)
	{
		var chatMessages = new List<ChatMessage>
		{
			new(role: "System", content: systemPrompt),
		};

		ra.systemPrompt = chatMessages;
		return ra;
	}


}
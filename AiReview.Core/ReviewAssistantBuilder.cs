using System.Collections.Generic;
using AiReview.Core.OpenAI;

namespace AiReview.Core;

public static class ReviewAssistantBuilder
{
	public static ReviewAssistant Create()
	{
		return new ReviewAssistant();
	}

	public static ReviewAssistant WithSystemPrompt(this ReviewAssistant ra, string systemPrompt)
	{
		var chatMessages = new List<ChatMessage>
		{
			new(role: "System", content: systemPrompt),
			new(role: "System", content: PromptDatabase.OutputPrompt),
		};

		ra.systemPrompt = chatMessages;
		return ra;
	}


	

}
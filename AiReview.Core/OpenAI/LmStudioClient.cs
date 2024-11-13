using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AiReview.Core.OpenAI
{
	public sealed class LmStudioClient
	{
		private readonly HttpClient httpClient;
		private readonly string apiUrl;

		public LmStudioClient(string apiUrl = "http://127.0.0.1:1234")
		{
			httpClient = new HttpClient();
			this.apiUrl = apiUrl;
		}

		public async Task<IEnumerable<ModelInfo>> GetModelsAsync()
		{
			var response = await httpClient.GetAsync($"{apiUrl}/v1/models");
			response.EnsureSuccessStatusCode();

			var models = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<IEnumerable<ModelInfo>>(models);
		}

		public async Task<CompletionResponse> GenerateTextAsync(string prompt, int maxTokens = 100, float temperature = 0.7f)
		{
			var requestBody = new
			{
				prompt,
				max_tokens = maxTokens,
				temperature
			};

			var response = await PostAsync<CompletionResponse>($"{apiUrl}/v1/completions", requestBody);
			return response;
		}

		public async Task<EmbeddingResponse> GenerateEmbeddingsAsync(string text)
		{
			var requestBody = new
			{
				input = text
			};

			var response = await PostAsync<EmbeddingResponse>($"{apiUrl}/v1/embeddings", requestBody);
			return response;
		}



		


		public async Task<ChatCompletionResponse> GenerateChatResponseAsync(IEnumerable<ChatMessage> messages, int maxTokens = 500, float temperature = 0)
		{
			var requestBody = new
			{
				messages,
				max_tokens = maxTokens,
				temperature
			};

			var response = await PostAsync<ChatCompletionResponse>($"{apiUrl}/v1/chat/completions", requestBody);
			return response;
		}

		private async Task<T> PostAsync<T>(string url, object requestBody)
		{
			var json = JsonConvert.SerializeObject(requestBody);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync(url, content);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(result);


		}
	}
}
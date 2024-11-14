using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AiReview.Core.OpenAI
{
	public sealed class LmStudioClient
	{
		public readonly string apiUrl;

		public LmStudioClient(string apiUrl = "http://127.0.0.1:1234")
		{
			this.apiUrl = apiUrl;
		}

		public async Task<ModelsResponse> GetModelsAsync()
		{
			using var httpClient = new HttpClient();
			var response = await httpClient.GetAsync($"{apiUrl}/v1/models");
			response.EnsureSuccessStatusCode();

			var models = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ModelsResponse>(models);
		}


		public static float ComputeCosineSimilarity(float[] embedding1, float[] embedding2)
		{
			// Step 5: Compute Cosine Similarity between two embeddings
			if (embedding1 == null || embedding2 == null)
			{
				throw new ArgumentException("Embeddings cannot be null");
			}

			// Normalize the embeddings to unit vectors
			var norm1 = Math.Sqrt(embedding1.Select(x => x * x).Sum());
			var norm2 = Math.Sqrt(embedding2.Select(x => x * x).Sum());

			// Compute the dot product
			var dotProduct = embedding1.Zip(embedding2, (x1, x2) => x1 * x2).Sum();

			// Cosine Similarity formula
			var cosineSimilarity = dotProduct / (norm1 * norm2);
			return (float)cosineSimilarity;
		}

		public async Task<float> ComputeSimilarity(string reference, string actual)
		{
			
			var p1 = await GenerateEmbeddingsAsync(reference);
			var p2 = await GenerateEmbeddingsAsync(actual);

			var e1 = p1.Data.SelectMany(x => x.Embedding).ToArray();
			var e2 = p2.Data.SelectMany(x => x.Embedding).ToArray();
			var cosSimilarity = ComputeCosineSimilarity(e1, e2);
			return cosSimilarity;
		}


		public async Task<CompletionResponse> GenerateTextAsync(string prompt, int maxTokens = 100,
			float temperature = 0.7f)
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


		public async Task<ChatCompletionResponse> GenerateChatResponseAsync(IEnumerable<ChatMessage> messages,
			int maxTokens = 1024, float temperature = 0)
		{
			const int seed = 1024;
			var requestBody = new
			{
				messages,
				max_tokens = maxTokens,
				temperature,
				seed,
			};

			var response = await PostAsync<ChatCompletionResponse>($"{apiUrl}/v1/chat/completions", requestBody);
			return response;
		}

		private async Task<T> PostAsync<T>(string url, object requestBody)
		{
			var json = JsonConvert.SerializeObject(requestBody);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMinutes(5);

			var response = await httpClient.PostAsync(url, content);
			response.EnsureSuccessStatusCode();
			var result = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(result);
		}


		


		public async Task GenerateChatResponseWithStreamingAsync(
			Func<ChatCompletionChunk, bool> continueFunction,
			IEnumerable<ChatMessage> messages,
			float temperature = 0,
			int maxTokens = 1024
		)
		{
			const int seed = 1024;
			var requestBody = new
			{
				messages,
				max_tokens = maxTokens,
				temperature = temperature,
				seed = seed,
				stream = true
			};

			var json = JsonConvert.SerializeObject(requestBody);
			using var content = new StringContent(json, Encoding.UTF8, "application/json");
			using var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/v1/chat/completions");
			request.Content = content;
			using var httpClient = new HttpClient();
			using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

			response.EnsureSuccessStatusCode();

			using var responseStream = await response.Content.ReadAsStreamAsync();
			using var reader = new StreamReader(responseStream);


			while (!reader.EndOfStream)
			{
				var line = await reader.ReadLineAsync();
				if (string.IsNullOrWhiteSpace(line)) continue;

				if (line.Contains("data:"))
				{
					var tokenResponse = JsonConvert.DeserializeObject<ChatCompletionChunk>(line.Replace("data:", ""));

					if (!continueFunction(tokenResponse))
					{
						break;
					}
				}
			}
		}

		public class ChatCompletionChunk
		{
			[JsonProperty("id")] public string Id { get; set; }

			[JsonProperty("object")] public string Object { get; set; }

			[JsonProperty("created")] public long Created { get; set; }

			[JsonProperty("model")] public string Model { get; set; }

			[JsonProperty("system_fingerprint")] public string SystemFingerprint { get; set; }

			[JsonProperty("choices")] public List<Choice> Choices { get; set; }

			public class Choice
			{
				[JsonProperty("index")] public int Index { get; set; }

				[JsonProperty("delta")] public Delta Delta { get; set; }

				[JsonProperty("logprobs")]
				public object Logprobs { get; set; } // Adjust type if you have a concrete definition

				[JsonProperty("finish_reason")] public string FinishReason { get; set; }
			}

			public class Delta
			{
				[JsonProperty("role")] public string Role { get; set; }

				[JsonProperty("content")] public string Content { get; set; }
			}
		}

		// Define a class to map streamed responses
	}
}
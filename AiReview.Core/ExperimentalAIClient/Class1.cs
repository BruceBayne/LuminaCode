namespace AiReview.Core.ExperimentalAIClient
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Message
    {
        [JsonProperty("role")] public string Role { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
    }

    public class ChatPayload
    {
        [JsonProperty("messages")] public List<Message> Messages { get; set; }

        [JsonProperty("conversation_id")] public string ConversationId { get; set; }

        [JsonProperty("frequency_penalty")] public int FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty")] public int PresencePenalty { get; set; }

        [JsonProperty("stop")] public string Stop { get; set; }

        [JsonProperty("temperature")] public int Temperature { get; set; }

        [JsonProperty("top_p")] public int TopP { get; set; }

        [JsonProperty("streaming")] public bool Streaming { get; set; }

        [JsonProperty("system_prompt")] public string SystemPrompt { get; set; }

        [JsonProperty("tools_to_use")] public List<string> ToolsToUse { get; set; }
    }

    public class ResultMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class ConversationResult
    {
        [JsonProperty("conversation_id")] public string ConversationId { get; set; }


        public List<ResultMessage> Messages { get; set; }
        public int TotalTokens { get; set; }
        public bool FinalResponse { get; set; }
        public string SharedId { get; set; }
    }

    public class ChatClient
    {
        private readonly string serverUrl;
        private readonly string token;

        public ChatClient(string serverUrl, string token)
        {
            this.serverUrl = serverUrl;
            this.token = token;
        }

        private static readonly HttpClient Client = new();

        public async Task<ConversationResult> SendMessageAsync(string systemPrompt, string userPrompt)
        {
            var payload = new ChatPayload
            {
                Messages =
                [
                    //new Message
                    //{
                    //    Role = "system",
                    //    Content = systemPrompt
                    //},
                    new Message
                    {
                        Role = "user",
                        Content = userPrompt
                    },
                ],
                ConversationId = null,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                Stop = "your_stop_condition",
                Temperature = 0,
                TopP = 0,
                Streaming = false,
                SystemPrompt = systemPrompt,
                ToolsToUse = ["search_uploaded_documents"]
            };


            var jsonPayload = JsonConvert.SerializeObject(payload);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await Client.PostAsync(serverUrl, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ConversationResult>(responseContent);
        }
    }
}
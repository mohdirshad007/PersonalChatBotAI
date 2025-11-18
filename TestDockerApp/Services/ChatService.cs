using System.Text;
using System.Text.Json;

namespace TestDockerApp.Services
{
    public class ChatService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ConversationMemory _memory;
        private readonly string _apiKey;
        private readonly string _openAiUrl;
        private readonly string _modelName;

        public ChatService(IHttpClientFactory httpFactory, ConversationMemory memory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _memory = memory;
            _apiKey = config["OPENAI_API_KEY"] ?? throw new InvalidOperationException("Set OPENAI_API_KEY in env.");
            _openAiUrl = config["OPENAI_API_URL"] ?? "https://api.openai.com/v1/chat/completions";
            _modelName = config["OPENAI_MODEL"] ?? "gpt-3.5-turbo";
        }

        public async Task<ChatResponseDto> SendMessageAsync(string sessionId, string userMessage)
        {
            // 1) Build message list: system + memory + new user message
            var messages = new List<OpenAiMessage>();
            messages.Add(new OpenAiMessage { Role = "system", Content = configSystemPrompt() });

            // Memory history
            var history = _memory.Get(sessionId);
            if (history != null)
            {
                messages.AddRange(history.Select(m =>
                    new OpenAiMessage { Role = m.Role, Content = m.Content }
                ));
            }

            // New message
            messages.Add(new OpenAiMessage { Role = "user", Content = userMessage });

            // 2) Request body
            var body = new
            {
                model = _modelName,
                messages = messages,
                temperature = 0.2,
                max_tokens = 800
            };

            var http = _httpFactory.CreateClient("openai");
            using var request = new HttpRequestMessage(HttpMethod.Post, _openAiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API error: {response.StatusCode} - {json}");
            }

            var openAiResp = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;

            // Extract assistant reply
            var assistantText = openAiResp
                .Choices?
                .FirstOrDefault()?
                .Message?
                .Content ?? string.Empty;

            // Save to memory
            _memory.Append(sessionId, new ConversationEntry("user", userMessage));
            _memory.Append(sessionId, new ConversationEntry("assistant", assistantText));

            // FIXED: use constructor → record requires argument
            return new ChatResponseDto(assistantText);
        }

        private string configSystemPrompt()
        {
            return "You are a helpful, concise assistant that answers user questions clearly. " +
                   "When asked for code, produce runnable examples and explain them briefly. " +
                   "If you don't know, say you don't know.";
        }
    }
}

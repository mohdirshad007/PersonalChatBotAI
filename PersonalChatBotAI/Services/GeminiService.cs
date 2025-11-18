using System.Text;
using System.Text.Json;

namespace TestDockerApp.Services
{
    public class GeminiService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ConversationMemory _memory;
        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly string _apiUrl;

        public GeminiService(IHttpClientFactory httpFactory, ConversationMemory memory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _memory = memory;

            _apiKey = config["GEMINI_API_KEY"] ?? throw new Exception("Missing GEMINI_API_KEY");
            _modelName = config["GEMINI_MODEL"] ?? "gemini-1.5-flash";
            _apiUrl = config["GEMINI_API_URL"] ?? "https://generativelanguage.googleapis.com/v1beta/models";
        }

        public async Task<ChatResponseDto> SendMessageAsync(string sessionId, string userMessage)
        {
            var messages = new List<object>();

            // Gemini does NOT support "system" role.
            // Convert system → user as recommended by Google.
            messages.Add(new
            {
                role = "user",
                parts = new[] { new { text = "You are a helpful assistant." } }
            });

            // Add conversation memory, but convert roles:
            // user      → user
            // assistant → model
            var history = _memory.Get(sessionId);
            if (history != null)
            {
                messages.AddRange(history.Select(m => new
                {
                    role = m.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.Content } }
                }));
            }

            // New user message
            messages.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            // Build the Gemini request body
            var body = new {
                contents = messages,
            };

            // Correct Gemini REST endpoint:
            var url = $"{_apiUrl}/{_modelName}:generateContent?key={_apiKey}";

            var http = _httpFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var response = await http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API Error: {json}");

            var result = JsonSerializer.Deserialize<GeminiResponse>(json);

            var reply = result?.candidates?[0]?.content?.parts?[0]?.text ?? "";

            // Save to memory with original roles
            _memory.Append(sessionId, new ConversationEntry("user", userMessage));
            _memory.Append(sessionId, new ConversationEntry("assistant", reply));

            return new ChatResponseDto(reply);
        }

    }

    public class GeminiResponse
    {
        public List<Candidate>? candidates { get; set; }
    }

    public class Candidate
    {
        public GeminiContent? content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart>? parts { get; set; }
    }

    public class GeminiPart
    {
        public string? text { get; set; }
    }
}

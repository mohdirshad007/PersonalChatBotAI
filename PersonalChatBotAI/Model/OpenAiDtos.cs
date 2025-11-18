using System.Text.Json.Serialization;

public class OpenAiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class OpenAiChatCompletionResponse
{
    public List<OpenAiChoice>? Choices { get; set; }
}

public class OpenAiChoice
{
    public OpenAiMessage? Message { get; set; }
}

public record ChatRequestDto(string SessionId, string Message);
public record ChatResponseDto(string Reply);
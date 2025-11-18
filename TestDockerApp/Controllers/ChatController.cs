using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestDockerApp.Services;

namespace TestDockerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _service;
    private readonly GeminiService _geminiService;

    public ChatController(ChatService service, GeminiService geminiService)
    {
        _service = service;
        _geminiService = geminiService;
    }

    /// <summary>
    /// Send message to AI chatbot.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.SessionId))
        {
            req = req with { SessionId = Guid.NewGuid().ToString() };
        }

        //var result = await _service.SendMessageAsync(req.SessionId, req.Message);
        var result = await _geminiService.SendMessageAsync(req.SessionId, req.Message);

        return Ok(new
        {
            sessionId = req.SessionId,
            reply = result.Reply
        });
    }
}

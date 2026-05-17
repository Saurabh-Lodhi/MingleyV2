using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Chat;
using Mingley.Application.DTOs.Common;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/chats")]
[Authorize]
[Produces("application/json")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chat;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ChatsController(IChatService chat) => _chat = chat;

    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var chats = await _chat.GetChatsAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { chats }));
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(Guid chatId, [FromQuery] int page = 1)
    {
        var messages = await _chat.GetMessagesAsync(Me, chatId, page);
        return Ok(ApiResponse<object>.Ok(new { messages }));
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest req)
    {
        var result = await _chat.SendMessageAsync(Me, chatId, req);
        return Ok(ApiResponse<SendMessageResponse>.Ok(result, "Message sent."));
    }

    [HttpPut("{chatId}/read")]
    public async Task<IActionResult> MarkRead(Guid chatId)
    {
        await _chat.MarkReadAsync(Me, chatId);
        return Ok(ApiResponse.Ok("Marked read."));
    }

    [HttpDelete("{chatId}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid chatId, Guid messageId)
    {
        await _chat.DeleteMessageAsync(Me, chatId, messageId);
        return Ok(ApiResponse.Ok("Message deleted."));
    }

    [HttpGet("{chatId}/quota")]
    public async Task<IActionResult> GetQuota(Guid chatId)
    {
        var q = await _chat.GetQuotaAsync(Me, chatId);
        return Ok(ApiResponse<ChatQuotaDto>.Ok(q));
    }
}

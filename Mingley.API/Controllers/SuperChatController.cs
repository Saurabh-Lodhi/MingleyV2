using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.SuperChat;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/superchat")]
[Authorize]
[Produces("application/json")]
public class SuperChatController : ControllerBase
{
    private readonly ISuperChatService _sc;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public SuperChatController(ISuperChatService sc) => _sc = sc;

    /// <summary>Send SuperChat — costs 500 coins. Girl gets 50% commission when she responds.</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendSuperChatRequest req)
    {
        var result = await _sc.SendAsync(Me, req);
        return Ok(ApiResponse<SendSuperChatResponse>.Ok(result, "SuperChat sent! Girl will be notified."));
    }

    /// <summary>Girl responds to SuperChat — creates match + credits commission instantly.</summary>
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> Respond(Guid id)
    {
        var result = await _sc.RespondAsync(Me, id);
        return Ok(ApiResponse<SuperChatDto>.Ok(result, "Match created! Commission credited to your earnings."));
    }

    [HttpGet("received")]
    public async Task<IActionResult> Received()
    {
        var list = await _sc.GetReceivedAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { superChats = list }));
    }

    [HttpGet("sent")]
    public async Task<IActionResult> Sent()
    {
        var list = await _sc.GetSentAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { superChats = list }));
    }
}

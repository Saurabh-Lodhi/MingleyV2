using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure.Services;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/calls")]
[Authorize]
[Produces("application/json")]
public class CallController : ControllerBase
{
    private readonly ICallService _calls;
    private readonly AgoraTokenService _agora;

    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public CallController(ICallService calls, AgoraTokenService agora)
    {
        _calls = calls;
        _agora = agora;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateCallRequest req)
    {
        var result = await _calls.InitiateCallAsync(Me, req.TargetId, req.CallType ?? "audio");
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("{callId}/answer")]
    public async Task<IActionResult> Answer(Guid callId)
    {
        var result = await _calls.AnswerCallAsync(Me, callId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("{callId}/end")]
    public async Task<IActionResult> End(Guid callId)
    {
        var result = await _calls.EndCallAsync(Me, callId);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost("{callId}/decline")]
    public async Task<IActionResult> Decline(Guid callId)
    {
        await _calls.DeclineCallAsync(Me, callId);
        return Ok(ApiResponse.Ok("Call declined."));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var calls = await _calls.GetHistoryAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { calls }));
    }
    [HttpGet("{callId}/agora-token")]
    public IActionResult GetAgoraToken(Guid callId)
    {
        var channelName = $"call_{callId}";
        uint uid = (uint)new Random().Next(1, 999999);

        var tokenData = _agora.GenerateToken(channelName, uid);

        return Ok(ApiResponse<object>.Ok(tokenData));
    }
}



public class InitiateCallRequest { public string TargetId { get; set; } = string.Empty; public string? CallType { get; set; } = "audio"; }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Subscription;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/subscriptions")]
[Authorize]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subs;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public SubscriptionsController(ISubscriptionService subs) => _subs = subs;

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> Plans()
    {
        var plans = await _subs.GetPlansAsync();
        return Ok(ApiResponse<object>.Ok(new { plans }));
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var s = await _subs.GetStatusAsync(Me);
        return Ok(ApiResponse<UserSubscriptionDto?>.Ok(s));
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req)
    {
        var r = await _subs.SubscribeAsync(Me, req);
        return Ok(ApiResponse<SubscribeResponse>.Ok(r, "Subscription activated!"));
    }

    [HttpPost("{subscriptionId}/cancel")]
    public async Task<IActionResult> Cancel(Guid subscriptionId, [FromBody] CancelRequest req)
    {
        await _subs.CancelAsync(Me, subscriptionId, req.Reason);
        return Ok(ApiResponse.Ok("Subscription cancelled."));
    }
}
public class CancelRequest { public string? Reason { get; set; } }

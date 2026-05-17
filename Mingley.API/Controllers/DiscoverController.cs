using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Discover;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1")]
[Authorize]
[Produces("application/json")]
public class DiscoverController : ControllerBase
{
    private readonly IDiscoverService _discover;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DiscoverController(IDiscoverService discover) => _discover = discover;

    [HttpGet("discover")]
    public async Task<IActionResult> Feed(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? gender = null,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        [FromQuery] int? maxDistance = null,
        [FromQuery] bool? onlineOnly = null)
    {
        var filters = new DiscoverFilters
        {
            Gender = gender,
            MinAge = minAge,
            MaxAge = maxAge,
            MaxDistance = maxDistance,
            OnlineOnly = onlineOnly,
        };
        var (users, pagination) = await _discover.GetFeedAsync(Me, page, Math.Min(limit, 50), filters);
        return Ok(ApiResponse<object>.Ok(new { users, pagination }));
    }

    [HttpPost("discover/swipe")]
    public async Task<IActionResult> Swipe([FromBody] SwipeRequest req)
    {
        var result = await _discover.SwipeAsync(Me, req);
        return Ok(ApiResponse<SwipeResponse>.Ok(result));
    }

    [HttpGet("matches")]
    public async Task<IActionResult> GetMatches([FromQuery] int page = 1, [FromQuery] int limit = 30)
    {
        var matches = await _discover.GetMatchesAsync(Me, page, limit);
        return Ok(ApiResponse<object>.Ok(new { matches }));
    }

    [HttpDelete("matches/{matchId}")]
    public async Task<IActionResult> Unmatch(Guid matchId)
    {
        await _discover.UnmatchAsync(Me, matchId);
        return Ok(ApiResponse.Ok("Unmatched."));
    }

    [HttpGet("discover/likes")]
    public async Task<IActionResult> WhoLikedMe()
    {
        var users = await _discover.GetWhoLikedMeAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { users }));
    }
}

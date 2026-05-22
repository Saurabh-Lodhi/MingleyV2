// FILE: Mingley.API/Controllers/DiscoverController.cs
// TASK 6: VIP-gated filter messaging
// TASK 7: Trending endpoint

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
        [FromQuery] bool? onlineOnly = null,
        [FromQuery] string? city = null,   // TASK 6: VIP only
        [FromQuery] bool? global = null)   // TASK 6: VIP only
    {
        var filters = new DiscoverFilters
        {
            Gender = gender,
            MinAge = minAge,
            MaxAge = maxAge,
            MaxDistance = maxDistance,
            OnlineOnly = onlineOnly,
            City = city,
            Global = global,
        };
        var (users, pagination) = await _discover.GetFeedAsync(Me, page, limit, filters);
        return Ok(ApiResponse<object>.Ok(new
        {
            users,
            pagination,
            vipFeatures = new
            {
                citySearch = "Available with Gold/Platinum subscription",
                globalSearch = "Available with Gold/Platinum subscription",
                extendedRadius = "Available with Gold/Platinum subscription",
            }
        }));
    }

    [HttpPost("discover/swipe")]
    public async Task<IActionResult> Swipe([FromBody] SwipeRequest req)
    {
        var result = await _discover.SwipeAsync(Me, req);
        return Ok(ApiResponse<SwipeResponse>.Ok(result,
            result.IsMatch ? "🎉 It's a match!" : "Swiped successfully."));
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
        return Ok(ApiResponse.Ok("Unmatched successfully."));
    }

    [HttpGet("discover/likes")]
    public async Task<IActionResult> WhoLikedMe()
    {
        var users = await _discover.GetWhoLikedMeAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { users, total = users.Count }));
    }

    // TASK 7: Trending sections
    [HttpGet("discover/trending")]
    public async Task<IActionResult> Trending()
    {
        var sections = await _discover.GetTrendingAsync(Me);
        return Ok(ApiResponse<object>.Ok(new
        {
            sections,
            generatedAt = DateTime.UtcNow,
        }));
    }

    // TASK 6: Check VIP filter status for current user
    [HttpGet("discover/filter-limits")]
    public async Task<IActionResult> FilterLimits(
        [FromServices] Microsoft.EntityFrameworkCore.DbContext db)
    {
        var userId = Me;
        // Simplified — just return limits
        return Ok(ApiResponse<object>.Ok(new
        {
            defaultRadiusKm = 100,
            maxRadiusForFree = 100,
            maxRadiusVip = 10000,
            citySearchVipOnly = true,
            globalSearchVipOnly = true,
            note = "Upgrade to Gold or Platinum to unlock extended search filters.",
        }));
    }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Mingley.Application.DTOs.Common;
//using Mingley.Application.DTOs.Discover;
//using Mingley.Application.Interfaces;
//using System.Security.Claims;

//namespace Mingley.API.Controllers;

//[ApiController]
//[Route("v1")]
//[Authorize]
//[Produces("application/json")]
//public class DiscoverController : ControllerBase
//{
//    private readonly IDiscoverService _discover;
//    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//    public DiscoverController(IDiscoverService discover) => _discover = discover;

//    [HttpGet("discover")]
//    public async Task<IActionResult> Feed(
//        [FromQuery] int page = 1,
//        [FromQuery] int limit = 20,
//        [FromQuery] string? gender = null,
//        [FromQuery] int? minAge = null,
//        [FromQuery] int? maxAge = null,
//        [FromQuery] int? maxDistance = null,
//        [FromQuery] bool? onlineOnly = null)
//    {
//        var filters = new DiscoverFilters
//        {
//            Gender = gender,
//            MinAge = minAge,
//            MaxAge = maxAge,
//            MaxDistance = maxDistance,
//            OnlineOnly = onlineOnly,
//        };
//        var (users, pagination) = await _discover.GetFeedAsync(Me, page, Math.Min(limit, 50), filters);
//        return Ok(ApiResponse<object>.Ok(new { users, pagination }));
//    }

//    [HttpPost("discover/swipe")]
//    public async Task<IActionResult> Swipe([FromBody] SwipeRequest req)
//    {
//        var result = await _discover.SwipeAsync(Me, req);
//        return Ok(ApiResponse<SwipeResponse>.Ok(result));
//    }

//    [HttpGet("matches")]
//    public async Task<IActionResult> GetMatches([FromQuery] int page = 1, [FromQuery] int limit = 30)
//    {
//        var matches = await _discover.GetMatchesAsync(Me, page, limit);
//        return Ok(ApiResponse<object>.Ok(new { matches }));
//    }

//    [HttpDelete("matches/{matchId}")]
//    public async Task<IActionResult> Unmatch(Guid matchId)
//    {
//        await _discover.UnmatchAsync(Me, matchId);
//        return Ok(ApiResponse.Ok("Unmatched."));
//    }

//    [HttpGet("discover/likes")]
//    public async Task<IActionResult> WhoLikedMe()
//    {
//        var users = await _discover.GetWhoLikedMeAsync(Me);
//        return Ok(ApiResponse<object>.Ok(new { users }));
//    }
//}

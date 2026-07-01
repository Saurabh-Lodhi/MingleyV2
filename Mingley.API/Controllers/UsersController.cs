using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mingley.Application.DTOs.Common;
using Mingley.Application.DTOs.Users;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public UsersController(IUserService users) => _users = users;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var p = await _users.GetMeAsync(Me);
        return p == null ? NotFound(ApiResponse<object>.Fail("Not found.", 404)) : Ok(ApiResponse<object>.Ok(p));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var p = await _users.GetUserAsync(id, Me);
        return p == null ? NotFound(ApiResponse<object>.Fail("User not found.", 404)) : Ok(ApiResponse<object>.Ok(p));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var p = await _users.UpdateProfileAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(p, "Profile updated."));
    }

    [HttpPut("me/interests")]
    public async Task<IActionResult> UpdateInterests([FromBody] UpdateInterestsRequest req)
    {
        await _users.UpdateInterestsAsync(Me, req.Interests);
        return Ok(ApiResponse.Ok("Interests updated."));
    }

    [HttpPut("me/preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest req)
    {
        await _users.UpdatePreferencesAsync(Me, req);
        return Ok(ApiResponse.Ok("Preferences updated."));
    }

    [HttpPut("me/location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest req)
    {
        await _users.UpdateLocationAsync(Me, req);
        return Ok(ApiResponse.Ok("Location updated."));
    }

    [HttpPut("me/travel-mode")]
    public async Task<IActionResult> SetTravelMode([FromBody] SetTravelModeRequest req)
    {
        await _users.SetTravelModeAsync(Me, req);
        return Ok(ApiResponse.Ok(req.Enabled ? "Travel Mode enabled." : "Travel Mode disabled."));
    }

    [HttpPost("me/images")]
    public async Task<IActionResult> AddImage([FromBody] AddImageRequest req)
    {
        var img = await _users.AddImageAsync(Me, req);
        return Ok(ApiResponse<object>.Ok(img, "Image added."));
    }

    [HttpDelete("me/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        await _users.DeleteImageAsync(Me, imageId);
        return Ok(ApiResponse.Ok("Image deleted."));
    }

    [HttpPut("me/images/reorder")]
    public async Task<IActionResult> ReorderImages([FromBody] ReorderImagesRequest req)
    {
        await _users.ReorderImagesAsync(Me, req);
        return Ok(ApiResponse.Ok("Images reordered."));
    }

    [HttpPut("me/images/{imageId}/primary")]
    public async Task<IActionResult> SetPrimary(Guid imageId)
    {
        await _users.SetPrimaryImageAsync(Me, imageId);
        return Ok(ApiResponse.Ok("Primary image updated."));
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(Guid id)
    {
        await _users.BlockUserAsync(Me, id);
        return Ok(ApiResponse.Ok("User blocked."));
    }

    [HttpDelete("{id}/block")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        await _users.UnblockUserAsync(Me, id);
        return Ok(ApiResponse.Ok("User unblocked."));
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlocked()
    {
        var list = await _users.GetBlockedUsersAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { users = list }));
    }

    [HttpPost("{id}/report")]
    public async Task<IActionResult> Report(Guid id, [FromBody] ReportRequest req)
    {
        await _users.ReportUserAsync(Me, id.ToString(), req.Reason, req.Description);
        return Ok(ApiResponse.Ok("Report submitted."));
    }

    [HttpPut("me/fcm-token")]
    public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest req)
    {
        await _users.UpdateFcmTokenAsync(Me, req.Token);
        return Ok(ApiResponse.Ok("FCM token updated."));
    }

    [HttpDelete("me/account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest req)
    {
        await _users.DeleteAccountAsync(Me, req);
        return Ok(ApiResponse.Ok("Account deleted. We're sorry to see you go."));
    }

    [HttpPost("me/cover-photo")]
    [HttpPut("me/cover-photo")]
    public async Task<IActionResult> UpdateCoverPhoto([FromBody] UpdateCoverPhotoRequest req)
    {
        await _users.UpdateCoverPhotoAsync(Me, req.CoverPhotoUrl);
        return Ok(ApiResponse.Ok("Cover photo updated."));
    }

    [HttpGet("me/cover-photo")]
    public async Task<IActionResult> GetCoverPhoto()
    {
        var url = await _users.GetCoverPhotoAsync(Me);
        return Ok(ApiResponse<object>.Ok(new { coverPhotoUrl = url }));
    }

    [HttpDelete("me/cover-photo")]
    public async Task<IActionResult> DeleteCoverPhoto()
    {
        await _users.DeleteCoverPhotoAsync(Me);
        return Ok(ApiResponse.Ok("Cover photo removed."));
    }

    [HttpPost("contacts")]
    public async Task<IActionResult> GetContacts([FromBody] ContactsRequest req)
    {
        var contacts = await _users.GetContactsOnAppAsync(req.PhoneNumbers);
        return Ok(ApiResponse<object>.Ok(new { contacts, total = contacts.Count }));
    }
}

public class ReportRequest { public string Reason { get; set; } = string.Empty; public string? Description { get; set; } }

public class UpdateCoverPhotoRequest { public string CoverPhotoUrl { get; set; } = string.Empty; }
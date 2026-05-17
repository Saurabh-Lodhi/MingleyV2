using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mingley.Application.DTOs.Common;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure.Persistence;
using System.Security.Claims;

namespace Mingley.API.Controllers;

[ApiController]
[Route("v1/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifs;
    private readonly MingleyDbContext _db;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public NotificationsController(INotificationService notifs, MingleyDbContext db)
    { _notifs = notifs; _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1)
    {
        var list = await _notifs.GetAllAsync(Me, page);
        return Ok(ApiResponse<object>.Ok(new { notifications = list }));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _db.Notifications
            .CountAsync(n => n.UserId == Me && !n.IsRead && !n.IsDeleted);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _notifs.MarkReadAsync(id, Me);
        return Ok(ApiResponse.Ok("Marked as read."));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notifs.MarkAllReadAsync(Me);
        return Ok(ApiResponse.Ok("All marked as read."));
    }
}

using Microsoft.EntityFrameworkCore;
using Mingley.Application.Interfaces;
using Mingley.Domain.Entities;
using Mingley.Infrastructure.Persistence;

namespace Mingley.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly MingleyDbContext _db;
    private readonly IHubNotifier _hub;

    public NotificationService(MingleyDbContext db, IHubNotifier hub) { _db = db; _hub = hub; }

    public async Task CreateAsync(Guid userId, string title, string body, string type, string? referenceId = null)
    {
        var n = new Notification { UserId = userId, Title = title, Body = body, Type = type, ReferenceId = referenceId };
        _db.Notifications.Add(n);
        await _db.SaveChangesAsync();
        // Push real-time
        await _hub.SendToUserAsync(userId.ToString(), "NewNotification", new
        {
            id = n.Id.ToString(), title, body, type, referenceId, isRead = false, createdAt = n.CreatedAt,
        });
    }

    public async Task<List<object>> GetAllAsync(Guid userId, int page)
    {
        const int ps = 30;
        var list = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * ps).Take(ps)
            .Select(n => (object)new { id = n.Id.ToString(), n.Title, n.Body, n.Type, n.IsRead, n.CreatedAt, n.ReferenceId })
            .ToListAsync();
        return list;
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var list = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        list.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync();
    }
}

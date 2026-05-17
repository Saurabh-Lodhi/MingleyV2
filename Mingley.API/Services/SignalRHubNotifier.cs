using Microsoft.AspNetCore.SignalR;
using Mingley.API.Hubs;
using Mingley.Application.Interfaces;

namespace Mingley.API.Services;

public class SignalRHubNotifier : IHubNotifier
{
    private readonly IHubContext<ChatHub> _chat;
    private readonly IHubContext<NotificationHub> _notif;

    public SignalRHubNotifier(IHubContext<ChatHub> chat, IHubContext<NotificationHub> notif)
    { _chat = chat; _notif = notif; }

    public Task SendToUserAsync(string userId, string method, object data) =>
        _chat.Clients.Group($"user_{userId}").SendAsync(method, data);

    public Task SendToGroupAsync(string group, string method, object data) =>
        _chat.Clients.Group(group).SendAsync(method, data);
}

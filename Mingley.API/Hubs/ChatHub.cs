using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Mingley.Application.Interfaces;
using System.Security.Claims;

namespace Mingley.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IUserService _users;
    public ChatHub(IUserService users) => _users = users;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await _users.SetOnlineStatusAsync(Guid.Parse(userId), true);
            // FIX: Broadcast online status to all connected clients
            await Clients.Others.SendAsync("UserOnlineStatus", new { userId, isOnline = true });
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await _users.SetOnlineStatusAsync(Guid.Parse(userId), false);
            // FIX: Broadcast offline status to all connected clients
            await Clients.Others.SendAsync("UserOnlineStatus", new { userId, isOnline = false });
        }
        await base.OnDisconnectedAsync(ex);
    }

    // Join chat room (call when opening a chat screen)
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
    }

    // FIX: Typing indicator — only send to others in the chat room
    public async Task Typing(string chatId, bool isTyping)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.OthersInGroup($"chat_{chatId}").SendAsync("Typing", new { userId, isTyping, chatId });
    }

    // WebRTC signalling relay for real calls
    public async Task CallSignal(string targetUserId, string signalType, string signalData)
    {
        var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.Group($"user_{targetUserId}").SendAsync("CallSignal", new { senderId, signalType, signalData });
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Real_TimeChat.Data;
using Real_TimeChat.Models;

namespace Real_TimeChat.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatDbContext _context;
    // Keep track of active connections mapped to UserIds
    private static readonly ConcurrentDictionary<string, int> _connections = new();

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            _connections[Context.ConnectionId] = userId;

            // Update user online status in DB
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = true;
                user.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Broadcast online status to everyone
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }

            // Join SignalR groups for all ChatRooms the user is in
            var userRooms = await _context.UserChatRooms
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ChatRoomId)
                .ToListAsync();

            foreach (var roomId in userRooms)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out int userId))
        {
            // Check if the user has any other active connections
            var hasOtherConnections = _connections.Values.Any(id => id == userId);
            if (!hasOtherConnections)
            {
                // Update user online status to offline in DB
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Broadcast offline status to everyone
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Explicitly join a group when added dynamically
    public async Task JoinRoom(int chatRoomId)
    {
        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            // Verify user is member of this room
            var isMember = await _context.UserChatRooms
                .AnyAsync(uc => uc.UserId == userId && uc.ChatRoomId == chatRoomId);

            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
            }
        }
    }

    // Broadcast message to a room
    public async Task SendMessage(int chatRoomId, string messageContent)
    {
        if (string.IsNullOrWhiteSpace(messageContent)) return;

        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            // Verify membership
            var isMember = await _context.UserChatRooms
                .AnyAsync(uc => uc.UserId == userId && uc.ChatRoomId == chatRoomId);

            if (!isMember) return;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Save message to database
            var message = new Message
            {
                ChatRoomId = chatRoomId,
                SenderId = userId,
                Content = messageContent,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast message payload to group
            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                chatRoomId = message.ChatRoomId,
                senderId = message.SenderId,
                senderName = user.Username,
                senderAvatar = user.AvatarUrl ?? "/avatars/avatar_1.svg",
                content = message.Content,
                sentAt = message.SentAt.ToString("o"), // ISO format
                formattedTime = message.SentAt.ToLocalTime().ToString("hh:mm tt")
            });
        }
    }

    // Typing State Broadcast
    public async Task SendTypingState(int chatRoomId, bool isTyping)
    {
        var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                await Clients.OthersInGroup(chatRoomId.ToString())
                    .SendAsync("ReceiveTypingState", chatRoomId, userId, user.Username, isTyping);
            }
        }
    }
}

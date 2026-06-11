using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_TimeChat.Data;
using Real_TimeChat.Models;

namespace Real_TimeChat.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class ChatController : Controller
{
    private readonly ChatDbContext _context;

    public ChatController(ChatDbContext context)
    {
        _context = context;
    }

    private int CurrentUserId
    {
        get
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);
            return userId;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var userId = CurrentUserId;

        // Get rooms that current user is a member of
        var rooms = await _context.ChatRooms
            .Where(r => r.UserChatRooms.Any(uc => uc.UserId == userId))
            .Include(r => r.UserChatRooms)
                .ThenInclude(uc => uc.User)
            .Include(r => r.Messages)
            .ToListAsync();

        var roomList = rooms.Select(r => {
            string? name = r.Name;
            string? avatar = "/avatars/group_default.svg";
            string? subtext = r.Description;
            bool isOnline = false;

            if (!r.IsGroup)
            {
                // For direct messages, name and avatar belong to the other user
                var otherMember = r.UserChatRooms.FirstOrDefault(uc => uc.UserId != userId)?.User;
                if (otherMember != null)
                {
                    name = otherMember.Username;
                    avatar = otherMember.AvatarUrl ?? "/avatars/avatar_1.svg";
                    isOnline = otherMember.IsOnline;
                    subtext = otherMember.IsOnline ? "Online" : $"Last seen {otherMember.LastSeen.ToLocalTime().ToString("g")}";
                }
            }

            var lastMessage = r.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

            return new
            {
                id = r.Id,
                name = name,
                avatar = avatar,
                isGroup = r.IsGroup,
                isOnline = isOnline,
                subtext = subtext,
                lastMessage = lastMessage?.Content ?? "No messages yet",
                lastMessageTime = lastMessage != null ? lastMessage.SentAt.ToLocalTime().ToString("hh:mm tt") : ""
            };
        }).ToList();

        return Json(roomList);
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(int roomId)
    {
        var userId = CurrentUserId;

        // Verify membership
        var isMember = await _context.UserChatRooms
            .AnyAsync(uc => uc.UserId == userId && uc.ChatRoomId == roomId);

        if (!isMember)
        {
            return Forbid();
        }

        var messages = await _context.Messages
            .Where(m => m.ChatRoomId == roomId)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                id = m.Id,
                chatRoomId = m.ChatRoomId,
                senderId = m.SenderId,
                senderName = m.Sender.Username,
                senderAvatar = m.Sender.AvatarUrl ?? "/avatars/avatar_1.svg",
                content = m.Content,
                sentAt = m.SentAt.ToString("o"),
                formattedTime = m.SentAt.ToLocalTime().ToString("hh:mm tt")
            })
            .ToListAsync();

        return Json(messages);
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Json(new List<object>());
        }

        var userId = CurrentUserId;
        var users = await _context.Users
            .Where(u => u.Id != userId && u.Username.ToLower().Contains(query.ToLower()))
            .Take(10)
            .Select(u => new
            {
                id = u.Id,
                username = u.Username,
                avatarUrl = u.AvatarUrl ?? "/avatars/avatar_1.svg",
                bio = u.Bio,
                isOnline = u.IsOnline
            })
            .ToListAsync();

        return Json(users);
    }

    [HttpPost]
    public async Task<IActionResult> StartPrivateChat(int otherUserId)
    {
        var userId = CurrentUserId;

        if (userId == otherUserId)
        {
            return BadRequest("Cannot start a chat with yourself.");
        }

        // Verify other user exists
        var otherUser = await _context.Users.FindAsync(otherUserId);
        if (otherUser == null)
        {
            return NotFound("User not found.");
        }

        // Check if there's already a private chat between these two users
        var existingRoomId = await _context.ChatRooms
            .Where(r => !r.IsGroup)
            .Where(r => r.UserChatRooms.Any(uc => uc.UserId == userId) && r.UserChatRooms.Any(uc => uc.UserId == otherUserId))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (existingRoomId != 0)
        {
            return Json(new { roomId = existingRoomId });
        }

        // Create new private room
        var chatRoom = new ChatRoom
        {
            IsGroup = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();

        // Add both users as members
        var member1 = new UserChatRoom { UserId = userId, ChatRoomId = chatRoom.Id, JoinedAt = DateTime.UtcNow };
        var member2 = new UserChatRoom { UserId = otherUserId, ChatRoomId = chatRoom.Id, JoinedAt = DateTime.UtcNow };

        _context.UserChatRooms.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        return Json(new { roomId = chatRoom.Id });
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Group name is required.");
        }

        var userId = CurrentUserId;

        // Create group chat room
        var chatRoom = new ChatRoom
        {
            Name = name,
            Description = description,
            IsGroup = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId
        };

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();

        // Add creator as member
        var membership = new UserChatRoom
        {
            UserId = userId,
            ChatRoomId = chatRoom.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserChatRooms.Add(membership);
        await _context.SaveChangesAsync();

        return Json(new { roomId = chatRoom.Id });
    }

    [HttpGet]
    public async Task<IActionResult> SearchRooms(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Json(new List<object>());
        }

        var userId = CurrentUserId;

        // Get public groups that the current user is NOT in
        var rooms = await _context.ChatRooms
            .Where(r => r.IsGroup && r.Name!.ToLower().Contains(query.ToLower()))
            .Where(r => !r.UserChatRooms.Any(uc => uc.UserId == userId))
            .Take(10)
            .Select(r => new
            {
                id = r.Id,
                name = r.Name,
                description = r.Description,
                memberCount = r.UserChatRooms.Count
            })
            .ToListAsync();

        return Json(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> JoinGroup(int roomId)
    {
        var userId = CurrentUserId;

        // Verify room exists and is group
        var room = await _context.ChatRooms.FindAsync(roomId);
        if (room == null || !room.IsGroup)
        {
            return NotFound("Group not found.");
        }

        // Check if already member
        var alreadyMember = await _context.UserChatRooms
            .AnyAsync(uc => uc.UserId == userId && uc.ChatRoomId == roomId);

        if (alreadyMember)
        {
            return Json(new { success = true, roomId = roomId });
        }

        // Add membership
        var membership = new UserChatRoom
        {
            UserId = userId,
            ChatRoomId = roomId,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserChatRooms.Add(membership);
        await _context.SaveChangesAsync();

        return Json(new { success = true, roomId = roomId });
    }
}

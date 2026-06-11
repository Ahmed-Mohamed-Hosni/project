using System;
using System.Collections.Generic;

namespace Real_TimeChat.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

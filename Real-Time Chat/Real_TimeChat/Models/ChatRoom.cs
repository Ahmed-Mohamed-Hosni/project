using System;
using System.Collections.Generic;

namespace Real_TimeChat.Models;

public class ChatRoom
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsGroup { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public virtual User? CreatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

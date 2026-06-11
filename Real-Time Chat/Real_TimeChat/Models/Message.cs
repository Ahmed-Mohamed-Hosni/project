using System;

namespace Real_TimeChat.Models;

public class Message
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public int SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}

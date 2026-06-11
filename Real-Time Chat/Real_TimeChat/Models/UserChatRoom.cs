using System;

namespace Real_TimeChat.Models;

public class UserChatRoom
{
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

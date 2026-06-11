using Microsoft.EntityFrameworkCore;
using Real_TimeChat.Models;

namespace Real_TimeChat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ChatRoom> ChatRooms { get; set; } = null!;
    public DbSet<UserChatRoom> UserChatRooms { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserChatRoom composite key
        modelBuilder.Entity<UserChatRoom>()
            .HasKey(uc => new { uc.UserId, uc.ChatRoomId });

        // User - UserChatRoom (Many-to-Many join)
        modelBuilder.Entity<UserChatRoom>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserChatRooms)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChatRoom - UserChatRoom (Many-to-Many join)
        modelBuilder.Entity<UserChatRoom>()
            .HasOne(uc => uc.ChatRoom)
            .WithMany(cr => cr.UserChatRooms)
            .HasForeignKey(uc => uc.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message - ChatRoom
        modelBuilder.Entity<Message>()
            .HasOne(m => m.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message - Sender (User)
        // Set to Restrict to avoid multiple cascade paths in SQL Server
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatRoom - CreatedBy (User)
        modelBuilder.Entity<ChatRoom>()
            .HasOne(cr => cr.CreatedBy)
            .WithMany()
            .HasForeignKey(cr => cr.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

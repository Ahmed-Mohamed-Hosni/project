using Microsoft.AspNetCore.Identity;

namespace TaskManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Board> Boards { get; set; } = new List<Board>();
        public ICollection<Card> AssignedCards { get; set; } = new List<Card>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

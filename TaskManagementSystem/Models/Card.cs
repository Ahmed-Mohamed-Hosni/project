using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Order { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int BoardListId { get; set; }
        public BoardList? BoardList { get; set; }

        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

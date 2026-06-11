using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        public string? BackgroundColor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.Models
{
    public class BoardList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        public int Order { get; set; }

        public int BoardId { get; set; }
        public Board? Board { get; set; }

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}

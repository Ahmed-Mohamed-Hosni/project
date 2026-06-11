using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class StockTransaction
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [Display(Name = "Quantity Change")]
        public int Quantity { get; set; } // Positive for addition, Negative for removal

        [Required]
        [StringLength(50)]
        [Display(Name = "Transaction Type")]
        public string Type { get; set; } = string.Empty; // e.g. "Stock In", "Stock Out", "Adjustment"

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Reference ID")]
        public string? ReferenceId { get; set; } // Can store Sale ID, Purchase Invoice, etc.
    }
}

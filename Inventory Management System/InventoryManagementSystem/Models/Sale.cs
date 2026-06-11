using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Sale Date")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount ($)")]
        public decimal TotalAmount { get; set; }

        [StringLength(100)]
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        [Display(Name = "Customer Phone")]
        public string? CustomerPhone { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Sales Person")]
        public string SalesPerson { get; set; } = "Ahmed Mohamed Hosni";

        // Navigation Property
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}

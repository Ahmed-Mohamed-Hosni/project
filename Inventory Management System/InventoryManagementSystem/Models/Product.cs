using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(150, ErrorMessage = "Product Name cannot exceed 150 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Cost Price is required")]
        [Range(0.01, 1000000.00, ErrorMessage = "Cost Price must be greater than zero")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost Price ($)")]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "Selling Price is required")]
        [Range(0.01, 1000000.00, ErrorMessage = "Selling Price must be greater than zero")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Selling Price ($)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required")]
        [Range(0, 100000, ErrorMessage = "Stock Quantity cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Reorder Level is required")]
        [Range(0, 10000, ErrorMessage = "Reorder Level cannot be negative")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

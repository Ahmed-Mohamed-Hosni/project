using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Product)
                .WithMany()
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data for Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Devices, gadgets, and tech gear" },
                new Category { Id = 2, Name = "Office Supplies", Description = "Pens, paper, furniture, and office items" },
                new Category { Id = 3, Name = "Apparel", Description = "Clothing, shoes, and wearable garments" }
            );

            // Seed Data for Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier 
                { 
                    Id = 1, 
                    Name = "Apex Tech Distributors", 
                    ContactName = "John Doe", 
                    Phone = "+1555123456", 
                    Email = "sales@apextech.com", 
                    Address = "100 Tech Blvd, Silicon Valley",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Supplier 
                { 
                    Id = 2, 
                    Name = "Global Office Wholesalers", 
                    ContactName = "Jane Smith", 
                    Phone = "+1555987654", 
                    Email = "info@globaloffice.com", 
                    Address = "456 Commerce Rd, Chicago",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Supplier 
                { 
                    Id = 3, 
                    Name = "Trend Apparel Co.", 
                    ContactName = "Mark Taylor", 
                    Phone = "+1555678123", 
                    Email = "orders@trendapparel.com", 
                    Address = "789 Fashion Ave, New York",
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // Seed Data for Products
            modelBuilder.Entity<Product>().HasData(
                new Product 
                { 
                    Id = 1, 
                    Name = "Wireless Gaming Mouse", 
                    SKU = "TECH-MOU-001", 
                    Description = "High precision wireless ergonomic gaming mouse", 
                    CostPrice = 25.00m, 
                    Price = 59.99m, 
                    StockQuantity = 45, 
                    ReorderLevel = 10, 
                    CategoryId = 1, 
                    SupplierId = 1, 
                    CreatedAt = new DateTime(2026, 1, 5)
                },
                new Product 
                { 
                    Id = 2, 
                    Name = "Mechanical Keyboard", 
                    SKU = "TECH-KEY-002", 
                    Description = "RGB Backlit blue switch mechanical keyboard", 
                    CostPrice = 45.00m, 
                    Price = 89.99m, 
                    StockQuantity = 8, // Low Stock on purpose for testing alerts
                    ReorderLevel = 12, 
                    CategoryId = 1, 
                    SupplierId = 1, 
                    CreatedAt = new DateTime(2026, 1, 5)
                },
                new Product 
                { 
                    Id = 3, 
                    Name = "Ergonomic Desk Chair", 
                    SKU = "OFFC-CHR-001", 
                    Description = "Adjustable lumber support high-back office chair", 
                    CostPrice = 90.00m, 
                    Price = 189.50m, 
                    StockQuantity = 15, 
                    ReorderLevel = 5, 
                    CategoryId = 2, 
                    SupplierId = 2, 
                    CreatedAt = new DateTime(2026, 1, 5)
                },
                new Product 
                { 
                    Id = 4, 
                    Name = "Designer Cotton T-Shirt", 
                    SKU = "CLTH-TSH-001", 
                    Description = "Premium organic cotton breathable crewneck shirt", 
                    CostPrice = 8.50m, 
                    Price = 24.99m, 
                    StockQuantity = 120, 
                    ReorderLevel = 20, 
                    CategoryId = 3, 
                    SupplierId = 3, 
                    CreatedAt = new DateTime(2026, 1, 5)
                }
            );
        }
    }
}

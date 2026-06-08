using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@ecommerce.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    Address = "123 Admin Street",
                    City = "Cairo",
                    PostalCode = "11511"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics", Description = "Smartphones, laptops, tablets and more" },
                    new Category { Name = "Clothing", Description = "Men's and women's fashion apparel" },
                    new Category { Name = "Home & Kitchen", Description = "Furniture, appliances and home decor" },
                    new Category { Name = "Sports & Outdoors", Description = "Fitness equipment, outdoor gear and sportswear" },
                    new Category { Name = "Books", Description = "Bestsellers, textbooks and e-books" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!await context.Products.AnyAsync())
            {
                var electronics = await context.Categories.FirstAsync(c => c.Name == "Electronics");
                var clothing = await context.Categories.FirstAsync(c => c.Name == "Clothing");
                var homeKitchen = await context.Categories.FirstAsync(c => c.Name == "Home & Kitchen");
                var sports = await context.Categories.FirstAsync(c => c.Name == "Sports & Outdoors");
                var books = await context.Categories.FirstAsync(c => c.Name == "Books");

                var products = new List<Product>
                {
                    new Product { Name = "Wireless Bluetooth Headphones", Description = "Premium noise-cancelling headphones with 30-hour battery life, deep bass, and crystal clear audio. Perfect for music lovers and professionals.", Price = 79.99m, ImageUrl = "/images/products/headphones.png", StockQuantity = 50, CategoryId = electronics.Id },
                    new Product { Name = "Smart Watch Pro", Description = "Advanced fitness tracker with heart rate monitor, GPS, sleep tracking, and 7-day battery life. Water resistant up to 50m.", Price = 199.99m, ImageUrl = "/images/products/smartwatch.png", StockQuantity = 35, CategoryId = electronics.Id },
                    new Product { Name = "Laptop Stand Aluminum", Description = "Ergonomic aluminum laptop stand with adjustable height and angle. Compatible with all laptops 10-17 inches.", Price = 45.99m, ImageUrl = "/images/products/laptopstand.png", StockQuantity = 100, CategoryId = electronics.Id },
                    new Product { Name = "USB-C Hub 7-in-1", Description = "Multi-port adapter with HDMI 4K, USB 3.0, SD card reader, and 100W power delivery. Essential for modern laptops.", Price = 34.99m, ImageUrl = "/images/products/usbhub.png", StockQuantity = 75, CategoryId = electronics.Id },
                    new Product { Name = "Classic Denim Jacket", Description = "Timeless denim jacket with comfortable fit, vintage wash, and durable construction. A wardrobe essential for all seasons.", Price = 59.99m, ImageUrl = "/images/products/denim.png", StockQuantity = 40, CategoryId = clothing.Id },
                    new Product { Name = "Running Shoes Ultra", Description = "Lightweight running shoes with responsive cushioning, breathable mesh upper, and excellent grip. Ideal for daily training.", Price = 89.99m, ImageUrl = "/images/products/shoes.png", StockQuantity = 60, CategoryId = sports.Id },
                    new Product { Name = "Stainless Steel Water Bottle", Description = "Double-wall vacuum insulated bottle keeps drinks cold for 24h or hot for 12h. BPA-free, 750ml capacity.", Price = 24.99m, ImageUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=400&h=400&fit=crop", StockQuantity = 120, CategoryId = sports.Id },
                    new Product { Name = "Modern Desk Lamp", Description = "LED desk lamp with 5 brightness levels, 3 color temperatures, USB charging port, and flexible gooseneck design.", Price = 39.99m, ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057ab6fe?w=400&h=400&fit=crop", StockQuantity = 80, CategoryId = homeKitchen.Id },
                    new Product { Name = "Non-Stick Cookware Set", Description = "Premium 10-piece cookware set with ceramic non-stick coating, oven safe up to 450°F. Dishwasher safe.", Price = 129.99m, ImageUrl = "https://images.unsplash.com/photo-1556909114-44e3e70034e2?w=400&h=400&fit=crop", StockQuantity = 25, CategoryId = homeKitchen.Id },
                    new Product { Name = "Programming Mastery Guide", Description = "Comprehensive guide covering C#, .NET, design patterns, and clean architecture. 500+ pages with practical examples.", Price = 29.99m, ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400&h=400&fit=crop", StockQuantity = 200, CategoryId = books.Id },
                };
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}

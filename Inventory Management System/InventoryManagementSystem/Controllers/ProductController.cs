using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, int? categoryId, bool? lowStock)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.SKU.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (lowStock.HasValue && lowStock.Value)
            {
                query = query.Where(p => p.StockQuantity <= p.ReorderLevel);
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["LowStockOnly"] = lowStock;

            return View(await query.ToListAsync());
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            ViewData["SupplierId"] = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SKU,Description,CostPrice,Price,StockQuantity,ReorderLevel,CategoryId,SupplierId")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Log the initial stock transaction
                if (product.StockQuantity > 0)
                {
                    var transaction = new StockTransaction
                    {
                        ProductId = product.Id,
                        Quantity = product.StockQuantity,
                        Type = "Stock In",
                        Description = "Initial stock upload during product creation",
                        TransactionDate = DateTime.Now
                    };
                    _context.StockTransactions.Add(transaction);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Product '{product.Name}' was created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SKU,Description,CostPrice,Price,StockQuantity,ReorderLevel,CategoryId,SupplierId,CreatedAt")] Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Track differences in stock quantity to log adjustments
                    var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (oldProduct != null && oldProduct.StockQuantity != product.StockQuantity)
                    {
                        int diff = product.StockQuantity - oldProduct.StockQuantity;
                        var transaction = new StockTransaction
                        {
                            ProductId = product.Id,
                            Quantity = diff,
                            Type = diff > 0 ? "Stock In" : "Stock Out",
                            Description = $"Stock updated in Product Editor (Previous: {oldProduct.StockQuantity}, New: {product.StockQuantity})",
                            TransactionDate = DateTime.Now
                        };
                        _context.StockTransactions.Add(transaction);
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Product '{product.Name}' was updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name", product.SupplierId);
            return View(product);
        }

        // GET: Product/AdjustStock/5
        public async Task<IActionResult> AdjustStock(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Product/AdjustStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(int id, int adjustmentQuantity, string adjustmentType, string? description)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (adjustmentQuantity == 0)
            {
                ModelState.AddModelError(string.Empty, "Adjustment quantity cannot be zero.");
                return View(product);
            }

            // Adjust quantity based on type
            int change = adjustmentQuantity;
            if (adjustmentType == "Stock Out" && change > 0)
            {
                change = -change;
            }
            else if (adjustmentType == "Adjustment" && change < 0 && Math.Abs(change) > product.StockQuantity)
            {
                ModelState.AddModelError(string.Empty, $"Cannot adjust stock below 0. Current stock is {product.StockQuantity}.");
                return View(product);
            }
            else if (adjustmentType == "Stock Out" && product.StockQuantity < Math.Abs(change))
            {
                ModelState.AddModelError(string.Empty, $"Insufficient stock to deduct. Current stock is {product.StockQuantity}.");
                return View(product);
            }

            product.StockQuantity += change;

            var transaction = new StockTransaction
            {
                ProductId = product.Id,
                Quantity = change,
                Type = adjustmentType,
                Description = string.IsNullOrEmpty(description) ? $"Manual stock adjustment ({adjustmentType})" : description,
                TransactionDate = DateTime.Now
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Stock for '{product.Name}' adjusted by {change} units successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Verify if product has been sold
                var hasSales = await _context.SaleItems.AnyAsync(s => s.ProductId == id);
                if (hasSales)
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete product because it has active sales records associated with it. Consider adjusting its stock to 0 instead.");
                    var fullProduct = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Supplier)
                        .FirstOrDefaultAsync(m => m.Id == id);
                    return View(fullProduct);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Name}' was deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

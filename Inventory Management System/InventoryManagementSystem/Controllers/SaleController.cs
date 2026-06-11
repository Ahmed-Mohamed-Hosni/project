using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Controllers
{
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SaleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sale (Invoice History)
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Sales.OrderByDescending(s => s.SaleDate).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.InvoiceNumber.Contains(searchString) || 
                                         (s.CustomerName != null && s.CustomerName.Contains(searchString)));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await query.ToListAsync());
        }

        // GET: Sale/Details/5 (View Invoice)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // GET: Sale/Create (POS Screen)
        public async Task<IActionResult> Create()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .ToListAsync();

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(products);
        }

        // POST: Sale/Create (Checkout API)
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return Json(new { success = false, message = "Cart is empty." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Generate Invoice Number: INV-YYYYMMDD-XXXX
                    string dateStr = DateTime.Now.ToString("yyyyMMdd");
                    int dailyCount = await _context.Sales.CountAsync(s => s.InvoiceNumber.StartsWith($"INV-{dateStr}")) + 1;
                    string invoiceNumber = $"INV-{dateStr}-{dailyCount:D4}";

                    var sale = new Sale
                    {
                        InvoiceNumber = invoiceNumber,
                        SaleDate = DateTime.Now,
                        CustomerName = string.IsNullOrEmpty(request.CustomerName) ? "Walk-in Customer" : request.CustomerName,
                        CustomerPhone = request.CustomerPhone,
                        SalesPerson = "Ahmed Mohamed Hosni", // Custom Developer Sign
                        TotalAmount = 0
                    };

                    _context.Sales.Add(sale);
                    await _context.SaveChangesAsync(); // Generates Sale.Id

                    decimal totalAmount = 0;

                    foreach (var item in request.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            return Json(new { success = false, message = $"Product with ID {item.ProductId} not found." });
                        }

                        if (product.StockQuantity < item.Quantity)
                        {
                            return Json(new { success = false, message = $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}" });
                        }

                        // Deduct Stock
                        product.StockQuantity -= item.Quantity;

                        // Create SaleItem
                        var saleItem = new SaleItem
                        {
                            SaleId = sale.Id,
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            TotalPrice = product.Price * item.Quantity
                        };

                        totalAmount += saleItem.TotalPrice;
                        _context.SaleItems.Add(saleItem);

                        // Log Stock Out Transaction
                        var stockTx = new StockTransaction
                        {
                            ProductId = product.Id,
                            Quantity = -item.Quantity,
                            Type = "Stock Out",
                            Description = $"Sold via Invoice {invoiceNumber}",
                            TransactionDate = DateTime.Now,
                            ReferenceId = sale.Id.ToString()
                        };
                        _context.StockTransactions.Add(stockTx);
                    }

                    sale.TotalAmount = totalAmount;
                    _context.Update(sale);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Json(new { success = true, saleId = sale.Id, invoiceNumber = sale.InvoiceNumber });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Transaction failed: {ex.Message}" });
                }
            }
        }

        // POST: Sale/Delete/5 (Cancel invoice & restore stock)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Restore stock for all products in invoice
                    foreach (var item in sale.SaleItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            
                            // Log Stock In (restoration)
                            var stockTx = new StockTransaction
                            {
                                ProductId = product.Id,
                                Quantity = item.Quantity,
                                Type = "Stock In",
                                Description = $"Restored stock from cancelled Invoice {sale.InvoiceNumber}",
                                TransactionDate = DateTime.Now,
                                ReferenceId = sale.Id.ToString()
                            };
                            _context.StockTransactions.Add(stockTx);
                        }
                    }

                    _context.Sales.Remove(sale);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = $"Invoice {sale.InvoiceNumber} was cancelled and stock levels were restored.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Failed to cancel invoice: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // Request payload models for checkout API
    public class CheckoutRequest
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public List<CheckoutItem>? Items { get; set; }
    }

    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

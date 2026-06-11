using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Controllers
{
    public class StockTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StockTransaction
        public async Task<IActionResult> Index(string searchString, string transactionType)
        {
            var query = _context.StockTransactions
                .Include(s => s.Product)
                .OrderByDescending(s => s.TransactionDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Product!.Name.Contains(searchString) || s.Product.SKU.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(s => s.Type == transactionType);
            }

            // Get unique transaction types for filter
            var types = new List<string> { "Stock In", "Stock Out", "Adjustment" };
            ViewData["TransactionTypes"] = new SelectList(types, transactionType);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentType"] = transactionType;

            return View(await query.ToListAsync());
        }
    }
}

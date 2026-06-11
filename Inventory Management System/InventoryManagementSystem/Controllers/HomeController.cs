using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            // 1. General Metrics
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            
            decimal stockValue = await _context.Products.SumAsync(p => p.StockQuantity * p.CostPrice);
            ViewBag.TotalStockValue = stockValue;

            decimal salesThisMonth = await _context.Sales
                .Where(s => s.SaleDate.Month == DateTime.Now.Month && s.SaleDate.Year == DateTime.Now.Year)
                .SumAsync(s => s.TotalAmount);
            ViewBag.SalesThisMonth = salesThisMonth;

            int lowStockCount = await _context.Products
                .CountAsync(p => p.StockQuantity <= p.ReorderLevel);
            ViewBag.LowStockCount = lowStockCount;

            // 2. Lists
            ViewBag.LowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.ReorderLevel)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .ToListAsync();

            // 3. Chart Data (Sales trends over the last 6 months)
            var monthlySalesList = new List<decimal>();
            var monthlySalesLabels = new List<string>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Today.AddMonths(-i);
                var total = await _context.Sales
                    .Where(s => s.SaleDate.Month == targetDate.Month && s.SaleDate.Year == targetDate.Year)
                    .SumAsync(s => s.TotalAmount);

                monthlySalesList.Add(total);
                monthlySalesLabels.Add(targetDate.ToString("MMMM yyyy"));
            }

            ViewBag.SalesChartData = monthlySalesList;
            ViewBag.SalesChartLabels = monthlySalesLabels;

            // 4. Chart Data (Category breakdown)
            var categoryBreakdown = await _context.Products
                .GroupBy(p => p.Category!.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.CategoryLabels = categoryBreakdown.Select(cb => cb.CategoryName).ToList();
            ViewBag.CategoryData = categoryBreakdown.Select(cb => cb.Count).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

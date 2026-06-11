using System.Diagnostics;
using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            ViewBag.Categories = categories;
            return View(featuredProducts);
        }

        public async Task<IActionResult> Shop(int? categoryId, string? search)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            var products = await productsQuery.ToListAsync();
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchQuery = search;
            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
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

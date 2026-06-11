using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await ImageFile.CopyToAsync(stream);
                    product.ImageUrl = "/images/products/" + fileName;
                }
                else
                {
                    product.ImageUrl = "/images/products/default.png";
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.Id) return NotFound();
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await ImageFile.CopyToAsync(stream);
                    product.ImageUrl = "/images/products/" + fileName;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

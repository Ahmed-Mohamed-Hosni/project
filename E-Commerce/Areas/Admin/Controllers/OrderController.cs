using E_Commerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var ordersQuery = _context.Orders.Include(o => o.User).AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == status);
            }
            
            var orders = await ordersQuery.OrderByDescending(o => o.OrderDate).ToListAsync();
            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            
            order.OrderStatus = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{id} status updated to {status}!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

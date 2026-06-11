using System.Security.Claims;
using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
                return RedirectToAction("Index", "Cart");

            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == cartId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
            return View(new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
                return RedirectToAction("Index", "Cart");

            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == cartId)
                .Include(c => c.Product)
                .ToListAsync();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.OrderStatus = "Pending";
            order.OrderTotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
            order.SessionId = cartId;

            // Remove model state errors for navigation properties
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cartItems;
                ViewBag.CartTotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
                return View(order);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order details
            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Count = cartItem.Quantity,
                    Price = cartItem.Product!.Price
                };
                _context.OrderDetails.Add(orderDetail);

                // Update stock
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cartItem.Quantity;
                }
            }

            // Clear cart
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Clear session cart id
            HttpContext.Session.Remove("CartId");

            return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}

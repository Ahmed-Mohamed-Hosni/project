using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCartId()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CartId")))
            {
                HttpContext.Session.SetString("CartId", Guid.NewGuid().ToString());
            }
            return HttpContext.Session.GetString("CartId")!;
        }

        public async Task<IActionResult> Index()
        {
            var cartId = GetCartId();
            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == cartId)
                .Include(c => c.Product)
                .ToListAsync();

            ViewBag.CartTotal = cartItems.Sum(c => c.Product!.Price * c.Quantity);
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int productId)
        {
            var cartId = GetCartId();
            var existingItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ShoppingCartId == cartId && c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    ShoppingCartId = cartId
                };
                _context.ShoppingCartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var cartId = GetCartId();
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ShoppingCartId == cartId && c.ProductId == productId);

            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> IncreaseQuantity(int productId)
        {
            var cartId = GetCartId();
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ShoppingCartId == cartId && c.ProductId == productId);

            if (item != null)
            {
                item.Quantity++;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DecreaseQuantity(int productId)
        {
            var cartId = GetCartId();
            var item = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ShoppingCartId == cartId && c.ProductId == productId);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(item);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
                return Json(new { count = 0 });

            var count = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == cartId)
                .SumAsync(c => c.Quantity);

            return Json(new { count });
        }
    }
}

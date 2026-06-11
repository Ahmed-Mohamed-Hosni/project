using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using _4._Booking_AppointmentSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace _4._Booking_AppointmentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BookingContext _context;

        public HomeController(ILogger<HomeController> logger, BookingContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            var services = await _context.Services.Include(s => s.Category).Take(6).ToListAsync();
            var providers = await _context.Providers.Take(3).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Services = services;
            ViewBag.Providers = providers;

            return View();
        }

        public IActionResult Developer()
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

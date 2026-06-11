using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _4._Booking_AppointmentSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace _4._Booking_AppointmentSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly BookingContext _context;

        public AdminController(BookingContext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard(string statusFilter)
        {
            var appointmentsQuery = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Provider)
                .AsQueryable();

            // Stats Calculations
            var totalAppointments = await _context.Appointments.CountAsync();
            var pendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
            var confirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed);
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            
            // Revenue is calculated from Confirmed or Completed appointments
            var totalRevenue = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed)
                .SumAsync(a => a.Service != null ? a.Service.Price : 0);

            // Filtering
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse(statusFilter, out AppointmentStatus filterEnum))
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.Status == filterEnum);
                }
            }

            var appointments = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            ViewBag.Total = totalAppointments;
            ViewBag.Pending = pendingAppointments;
            ViewBag.Confirmed = confirmedAppointments;
            ViewBag.Completed = completedAppointments;
            ViewBag.Revenue = totalRevenue;
            ViewBag.CurrentFilter = statusFilter;

            return View(appointments);
        }

        // POST: Admin/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            appointment.Status = status;
            await _context.SaveChangesAsync();

            TempData["AdminSuccessMessage"] = $"Appointment {appointment.BookingCode} status successfully updated to {status}.";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Admin/Services
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.Include(s => s.Category).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(services);
        }

        // POST: Admin/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(string name, string description, decimal price, int durationMinutes, int categoryId)
        {
            if (string.IsNullOrEmpty(name) || price < 0 || durationMinutes <= 0 || categoryId <= 0)
            {
                TempData["AdminErrorMessage"] = "Invalid service input parameters. Please check and retry.";
                return RedirectToAction(nameof(Services));
            }

            var service = new Service
            {
                Name = name,
                Description = description ?? string.Empty,
                Price = price,
                DurationMinutes = durationMinutes,
                CategoryId = categoryId
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            TempData["AdminSuccessMessage"] = $"New service '{name}' successfully created.";
            return RedirectToAction(nameof(Services));
        }

        // POST: Admin/DeleteService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Check if there are any appointments referencing this service
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceId == id);
            if (hasAppointments)
            {
                TempData["AdminErrorMessage"] = "Cannot delete this service because there are existing appointments referencing it.";
                return RedirectToAction(nameof(Services));
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            TempData["AdminSuccessMessage"] = "Service successfully deleted.";
            return RedirectToAction(nameof(Services));
        }

        // GET: Admin/Providers
        public async Task<IActionResult> Providers()
        {
            var providers = await _context.Providers.ToListAsync();
            return View(providers);
        }

        // POST: Admin/AddProvider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProvider(string name, string title, string bio, string email, string phone, double rating)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(title))
            {
                TempData["AdminErrorMessage"] = "Name and Title are required to register a specialist.";
                return RedirectToAction(nameof(Providers));
            }

            var provider = new Provider
            {
                Name = name,
                Title = title,
                Bio = bio ?? string.Empty,
                Email = email ?? string.Empty,
                Phone = phone ?? string.Empty,
                Rating = rating >= 1.0 && rating <= 5.0 ? rating : 5.0,
                ImagePath = "/images/default-avatar.png"
            };

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            // Set up default schedule (Mon-Fri 9-5) for new provider
            for (int day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
            {
                _context.ProviderSchedules.Add(new ProviderSchedule
                {
                    ProviderId = provider.Id,
                    DayOfWeek = (DayOfWeek)day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsActive = true
                });
            }
            await _context.SaveChangesAsync();

            TempData["AdminSuccessMessage"] = $"Specialist '{name}' successfully registered with default Monday-Friday working schedules.";
            return RedirectToAction(nameof(Providers));
        }

        // POST: Admin/DeleteProvider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProvider(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null)
            {
                return NotFound();
            }

            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ProviderId == id);
            if (hasAppointments)
            {
                TempData["AdminErrorMessage"] = "Cannot delete this specialist because there are active bookings associated with them.";
                return RedirectToAction(nameof(Providers));
            }

            // Remove provider schedules first
            var schedules = await _context.ProviderSchedules.Where(s => s.ProviderId == id).ToListAsync();
            _context.ProviderSchedules.RemoveRange(schedules);

            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();

            TempData["AdminSuccessMessage"] = "Specialist successfully removed.";
            return RedirectToAction(nameof(Providers));
        }
    }
}

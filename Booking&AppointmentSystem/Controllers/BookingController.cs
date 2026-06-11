using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _4._Booking_AppointmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _4._Booking_AppointmentSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingContext _context;

        public BookingController(BookingContext context)
        {
            _context = context;
        }

        // GET: Booking/Book
        public async Task<IActionResult> Book(int? categoryId, int? serviceId, int? providerId)
        {
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedServiceId = serviceId;
            ViewBag.SelectedProviderId = providerId;

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // AJAX GET: Booking/GetServicesByCategory?categoryId=1
        [HttpGet]
        public async Task<IActionResult> GetServicesByCategory(int categoryId)
        {
            var services = await _context.Services
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new { s.Id, s.Name, s.Description, s.Price, s.DurationMinutes })
                .ToListAsync();
            return Json(services);
        }

        // AJAX GET: Booking/GetProvidersByService?serviceId=1
        [HttpGet]
        public async Task<IActionResult> GetProvidersByService(int serviceId)
        {
            var providers = await _context.ProviderServices
                .Where(ps => ps.ServiceId == serviceId)
                .Select(ps => new { 
                    Id = ps.Provider!.Id, 
                    Name = ps.Provider.Name, 
                    Title = ps.Provider.Title, 
                    Rating = ps.Provider.Rating,
                    Bio = ps.Provider.Bio
                })
                .ToListAsync();
            return Json(providers);
        }

        // AJAX GET: Booking/GetAvailableSlots?providerId=1&date=2026-06-12&serviceId=1
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int providerId, string date, int serviceId)
        {
            if (!DateOnly.TryParse(date, out DateOnly appointmentDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD.");
            }

            // Get day of week
            var dayOfWeek = appointmentDate.DayOfWeek;

            // Get provider schedule for this day
            var schedule = await _context.ProviderSchedules
                .FirstOrDefaultAsync(ps => ps.ProviderId == providerId && ps.DayOfWeek == dayOfWeek && ps.IsActive);

            if (schedule == null)
            {
                return Json(new List<string>()); // Provider not working this day
            }

            // Get service duration
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return BadRequest("Service not found.");
            }

            // Get already booked slots for this provider on this date
            var bookedSlots = await _context.Appointments
                .Where(a => a.ProviderId == providerId && a.AppointmentDate == appointmentDate && a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.TimeSlot)
                .ToListAsync();

            // Generate time slots (every 30 minutes)
            var finalSlots = new List<object>();
            var currentTime = schedule.StartTime;
            var endTime = schedule.EndTime;
            var slotInterval = TimeSpan.FromMinutes(service.DurationMinutes);
            int safetyCounter = 0;
            
            while (currentTime + slotInterval <= endTime && safetyCounter < 100)
            {
                safetyCounter++;
                var currentSlotStart = currentTime;
                var currentSlotEnd = currentTime + slotInterval;

                // Check if this slot overlaps with any booked slots
                // An overlap occurs if: StartA < EndB AND StartB < EndA
                bool isBooked = false;
                foreach (var bookedTime in bookedSlots)
                {
                    // Booked slot runs from bookedTime to bookedTime + serviceDuration (let's assume same duration for simplicity, or get the actual booked service duration)
                    // For safety, let's assume the booked slots are exact match or overlap
                    var bookedStart = bookedTime;
                    var bookedEnd = bookedTime.Add(slotInterval); // assumption

                    if (currentSlotStart < bookedEnd && bookedStart < currentSlotEnd)
                    {
                        isBooked = true;
                        break;
                    }
                }

                bool isPast = false;
                if (appointmentDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    // If timezone is local, use local time
                    var nowTime = DateTime.Now.TimeOfDay;
                    if (currentSlotStart <= nowTime)
                    {
                        isPast = true;
                    }
                }

                finalSlots.Add(new {
                    Time = currentSlotStart.ToString(@"hh\:mm"),
                    Available = !isBooked && !isPast
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(30)); // Offer slots every 30 minutes
            }

            return Json(finalSlots);
        }

        // POST: Booking/Confirm
        [HttpPost]
        public async Task<IActionResult> Confirm(string customerName, string customerEmail, string customerPhone, string customerNotes, int serviceId, int providerId, string appointmentDate, string timeSlot)
        {
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerEmail) || string.IsNullOrEmpty(customerPhone) || string.IsNullOrEmpty(appointmentDate) || string.IsNullOrEmpty(timeSlot))
            {
                return BadRequest("Required form fields are missing.");
            }

            if (!DateOnly.TryParse(appointmentDate, out DateOnly date))
            {
                return BadRequest("Invalid appointment date.");
            }

            if (!TimeSpan.TryParse(timeSlot, out TimeSpan time))
            {
                return BadRequest("Invalid time slot.");
            }

            // Verify availability in DB to prevent double booking
            var isOverlapped = await _context.Appointments
                .AnyAsync(a => a.ProviderId == providerId && 
                               a.AppointmentDate == date && 
                               a.TimeSlot == time && 
                               a.Status != AppointmentStatus.Cancelled);

            if (isOverlapped)
            {
                TempData["ErrorMessage"] = "The selected time slot is no longer available. Please select another slot.";
                return RedirectToAction("Book");
            }

            // Generate booking code
            var random = new Random();
            var bookingCode = "BK-" + random.Next(100000, 999999).ToString();

            var appointment = new Appointment
            {
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                CustomerNotes = customerNotes ?? string.Empty,
                ServiceId = serviceId,
                ProviderId = providerId,
                AppointmentDate = date,
                TimeSlot = time,
                BookingCode = bookingCode,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { code = bookingCode });
        }

        // GET: Booking/Success?code=BK-123456
        public async Task<IActionResult> Success(string code)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.BookingCode == code);

            if (appointment == null)
            {
                return NotFound("Booking not found.");
            }

            return View(appointment);
        }

        // GET: Booking/Track
        public async Task<IActionResult> Track(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.BookingCode == code.Trim());

            if (appointment == null)
            {
                ViewBag.ErrorMessage = "No appointment found matching the reference code provided. Please check the code and try again.";
                return View();
            }

            return View("Status", appointment);
        }

        // POST: Booking/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string code)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.BookingCode == code);

            if (appointment == null)
            {
                return NotFound("Booking not found.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your appointment has been successfully cancelled.";
            return RedirectToAction("Track", new { code = code });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly HospitalDbContext _context;

        public DashboardController(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Statistics
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync();
            
            var today = DateTime.Today;
            ViewBag.TodayAppointmentsCount = await _context.Appointments
                .CountAsync(a => a.AppointmentDate == today);
                
            ViewBag.PendingBillsCount = await _context.Bills
                .CountAsync(b => b.PaymentStatus == "Unpaid");

            ViewBag.PendingBillsTotal = await _context.Bills
                .Where(b => b.PaymentStatus == "Unpaid")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            // Upcoming Appointments
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= today)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .Take(5)
                .ToListAsync();

            // Status Breakdown for Chart
            var statusCounts = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            ViewBag.StatusLabels = new[] { "Pending", "Approved", "Completed", "Cancelled" };
            ViewBag.StatusValues = new[]
            {
                statusCounts.TryGetValue("Pending", out var p) ? p : 0,
                statusCounts.TryGetValue("Approved", out var a) ? a : 0,
                statusCounts.TryGetValue("Completed", out var c) ? c : 0,
                statusCounts.TryGetValue("Cancelled", out var cn) ? cn : 0
            };

            return View(upcomingAppointments);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HospitalDbContext _context;

        public ReportsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            // 1. Billing Overview
            var totalBilled = await _context.Bills.SumAsync(b => (decimal?)b.TotalAmount) ?? 0;
            var totalPaid = await _context.Bills.Where(b => b.PaymentStatus == "Paid").SumAsync(b => (decimal?)b.TotalAmount) ?? 0;
            var totalUnpaid = await _context.Bills.Where(b => b.PaymentStatus == "Unpaid").SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            ViewBag.TotalBilled = totalBilled;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.TotalUnpaid = totalUnpaid;

            // 2. Doctor Appointment Load
            var doctorLoads = await _context.Appointments
                .Include(a => a.Doctor)
                .GroupBy(a => a.Doctor!.Name)
                .Select(g => new { DoctorName = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.DoctorLoadLabels = doctorLoads.Select(d => d.DoctorName).ToArray();
            ViewBag.DoctorLoadValues = doctorLoads.Select(d => d.Count).ToArray();

            // 3. Specialty load
            var specialtyLoads = await _context.Appointments
                .Include(a => a.Doctor)
                .GroupBy(a => a.Doctor!.Specialization)
                .Select(g => new { Specialty = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.SpecialtyLabels = specialtyLoads.Select(s => s.Specialty).ToArray();
            ViewBag.SpecialtyValues = specialtyLoads.Select(s => s.Count).ToArray();

            // 4. Patient Gender Distribution
            var genderDist = await _context.Patients
                .GroupBy(p => p.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Gender, x => x.Count);

            ViewBag.GenderLabels = new[] { "Male", "Female" };
            ViewBag.GenderValues = new[]
            {
                genderDist.TryGetValue("Male", out var m) ? m : 0,
                genderDist.TryGetValue("Female", out var f) ? f : 0
            };

            // 5. Monthly Revenue Trends (Past 6 Months)
            var sixMonthsAgo = DateTime.Today.AddMonths(-6);
            var monthlyRevenue = await _context.Bills
                .Where(b => b.PaymentDate != null && b.PaymentDate >= sixMonthsAgo)
                .ToListAsync();

            var revenueTrends = monthlyRevenue
                .GroupBy(b => new { Month = b.PaymentDate!.Value.ToString("yyyy-MM") })
                .Select(g => new { Month = g.Key.Month, Revenue = g.Sum(x => x.TotalAmount) })
                .OrderBy(g => g.Month)
                .ToList();

            ViewBag.RevenueLabels = revenueTrends.Select(r => r.Month).ToArray();
            ViewBag.RevenueValues = revenueTrends.Select(r => r.Revenue).ToArray();

            // Recent Invoices for list
            var recentBills = await _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.Appointment)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(recentBills);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly HospitalDbContext _context;

        public SchedulesController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Schedules
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.DoctorSchedules
                .Include(d => d.Doctor)
                .OrderBy(s => s.Doctor!.Name)
                .ThenBy(s => s.DayOfWeek)
                .ToListAsync();
            return View(schedules);
        }

        // GET: Schedules/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId,DayOfWeek,StartTime,EndTime,SlotDurationMinutes")] DoctorSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "Name", schedule.DoctorId);
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.DoctorSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

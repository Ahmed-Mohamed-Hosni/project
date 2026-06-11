using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly HospitalDbContext _context;

        public AppointmentsController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(int? doctorId, string status, string date)
        {
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "Name");

            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
                ViewBag.SelectedDoctor = doctorId;
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
                ViewBag.SelectedStatus = status;
            }

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
            {
                query = query.Where(a => a.AppointmentDate == parsedDate.Date);
                ViewBag.SelectedDate = date;
            }

            var appointments = await query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.TimeSlot).ToListAsync();
            return View(appointments);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "Name");
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,DoctorId,AppointmentDate,TimeSlot,Notes")] Appointment appointment)
        {
            // Set status to Pending by default
            appointment.Status = "Pending";
            appointment.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // Auto-create a pending bill for the appointment
                var bill = new Bill
                {
                    PatientId = appointment.PatientId,
                    AppointmentId = appointment.Id,
                    TotalAmount = 150.00m, // standard doctor fee
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Now
                };
                _context.Add(bill);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = new SelectList(await _context.Patients.ToListAsync(), "Id", "Name", appointment.PatientId);
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "Id", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // POST: Appointments/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Approved";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Appointments/Complete/5
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Completed";
            
            // Check if there is a bill, make sure it is generated
            var billExists = await _context.Bills.AnyAsync(b => b.AppointmentId == id);
            if (!billExists)
            {
                var bill = new Bill
                {
                    PatientId = appointment.PatientId,
                    AppointmentId = appointment.Id,
                    TotalAmount = 150.00m,
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Now
                };
                _context.Add(bill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CreateRecord", "Appointments", new { appointmentId = id });
        }

        // GET: Appointments/CreateRecord
        public async Task<IActionResult> CreateRecord(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            ViewBag.Appointment = appointment;
            var record = new MedicalRecord
            {
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                RecordDate = DateTime.Now
            };

            return View(record);
        }

        // POST: Appointments/CreateRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRecord(int appointmentId, [Bind("PatientId,DoctorId,Diagnosis,Treatment,PrescribedMedications,Notes")] MedicalRecord record)
        {
            if (ModelState.IsValid)
            {
                record.RecordDate = DateTime.Now;
                _context.Add(record);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            ViewBag.Appointment = appointment;

            return View(record);
        }

        // POST: Appointments/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = "Cancelled";
            
            // Cancel unpaid bills related to this appointment
            var bills = await _context.Bills.Where(b => b.AppointmentId == id && b.PaymentStatus == "Unpaid").ToListAsync();
            foreach (var bill in bills)
            {
                _context.Bills.Remove(bill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/GetAvailableSlots?doctorId=1&date=2026-06-12
        [HttpGet]
        public async Task<JsonResult> GetAvailableSlots(int doctorId, string date)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
            {
                return Json(new { success = false, message = "Invalid date format." });
            }

            var dayOfWeek = selectedDate.DayOfWeek;
            
            // 1. Get doctor schedule for this day of week
            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek);

            if (schedule == null)
            {
                return Json(new { success = true, slots = new string[] { } });
            }

            // 2. Generate all slots
            var slots = new List<TimeSpan>();
            var current = schedule.StartTime;
            while (current + TimeSpan.FromMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
            {
                slots.Add(current);
                current = current.Add(TimeSpan.FromMinutes(schedule.SlotDurationMinutes));
            }

            // 3. Get already booked active slots for this doctor on this day
            var bookedTimes = await _context.Appointments
                .Where(a => a.DoctorId == doctorId 
                         && a.AppointmentDate == selectedDate.Date 
                         && a.Status != "Cancelled")
                .Select(a => a.TimeSlot)
                .ToListAsync();

            // 4. Filter out booked slots
            var availableSlots = slots
                .Where(s => !bookedTimes.Contains(s))
                .Select(s => s.ToString(@"hh\:mm"))
                .ToList();

            return Json(new { success = true, slots = availableSlots });
        }
    }
}

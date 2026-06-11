using System;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Models
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships & Avoid Circular Cascades
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Patient)
                .WithMany(p => p.Bills)
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Appointment)
                .WithMany(a => a.Bills)
                .HasForeignKey(b => b.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Data
            // 1. Doctors
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Ahmed Mohamed Hosni",
                    Email = "ahmed.hosni@hospital.com",
                    Phone = "+201012345678",
                    Specialization = "Chief Cardiologist & System Architect",
                    Biography = "Founder of the digital health platform. Specializes in advanced cardiac care, cardiovascular diagnostics, and custom enterprise system engineering.",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?q=80&w=200&auto=format&fit=crop"
                },
                new Doctor
                {
                    Id = 2,
                    Name = "Dr. Sarah Jenkins",
                    Email = "sarah.jenkins@hospital.com",
                    Phone = "+15550192834",
                    Specialization = "Pediatrics",
                    Biography = "Over 12 years of experience in pediatric care. Dedicated to providing compassionate health support for children from infancy through adolescence.",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1594824813573-246434de83fb?q=80&w=200&auto=format&fit=crop"
                },
                new Doctor
                {
                    Id = 3,
                    Name = "Dr. Robert Chen",
                    Email = "robert.chen@hospital.com",
                    Phone = "+15550284756",
                    Specialization = "Orthopedic Surgery",
                    Biography = "Expert in sports medicine and reconstructive orthopedics. Focused on helping patients regain mobility and live pain-free lives.",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?q=80&w=200&auto=format&fit=crop"
                },
                new Doctor
                {
                    Id = 4,
                    Name = "Dr. Emily Taylor",
                    Email = "emily.taylor@hospital.com",
                    Phone = "+15550375869",
                    Specialization = "Neurology",
                    Biography = "Specializes in neurodegenerative disorders, sleep medicine, and diagnostic neurophysiology. Author of multiple clinical research journals.",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?q=80&w=200&auto=format&fit=crop"
                }
            );

            // 2. Doctor Schedules
            modelBuilder.Entity<DoctorSchedule>().HasData(
                // Doctor 1 (Ahmed Mohamed Hosni) - Sunday, Tuesday, Thursday
                new DoctorSchedule { Id = 1, DoctorId = 1, DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDurationMinutes = 30 },
                new DoctorSchedule { Id = 2, DoctorId = 1, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDurationMinutes = 30 },
                new DoctorSchedule { Id = 3, DoctorId = 1, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(15, 0, 0), SlotDurationMinutes = 30 },

                // Doctor 2 (Sarah Jenkins) - Monday, Wednesday
                new DoctorSchedule { Id = 4, DoctorId = 2, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(16, 30, 0), SlotDurationMinutes = 30 },
                new DoctorSchedule { Id = 5, DoctorId = 2, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(16, 30, 0), SlotDurationMinutes = 30 },

                // Doctor 3 (Robert Chen) - Monday, Thursday
                new DoctorSchedule { Id = 6, DoctorId = 3, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), SlotDurationMinutes = 45 },
                new DoctorSchedule { Id = 7, DoctorId = 3, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0), SlotDurationMinutes = 45 },

                // Doctor 4 (Emily Taylor) - Tuesday, Wednesday
                new DoctorSchedule { Id = 8, DoctorId = 4, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDurationMinutes = 30 },
                new DoctorSchedule { Id = 9, DoctorId = 4, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), SlotDurationMinutes = 30 }
            );

            // 3. Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    Name = "Johnathan Doe",
                    Email = "john.doe@gmail.com",
                    Phone = "+15551234567",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1985, 5, 12),
                    Address = "123 Main St, New York, NY",
                    MedicalHistory = "Chronic hypertension diagnosed in 2021. Non-smoker.",
                    CreatedAt = DateTime.Now.AddDays(-60)
                },
                new Patient
                {
                    Id = 2,
                    Name = "Mariam Hassan Al-Masry",
                    Email = "mariam.hassan@yahoo.com",
                    Phone = "+201287654321",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1993, 10, 24),
                    Address = "45 El-Tahrir St, Cairo, Egypt",
                    MedicalHistory = "Mild seasonal allergies to pollen. Normal blood pressure.",
                    CreatedAt = DateTime.Now.AddDays(-45)
                },
                new Patient
                {
                    Id = 3,
                    Name = "David Miller",
                    Email = "david.miller@gmail.com",
                    Phone = "+15559876543",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1978, 2, 4),
                    Address = "789 Pine Ave, Seattle, WA",
                    MedicalHistory = "ACL reconstruction in left knee (2018). Occasional asthma.",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Patient
                {
                    Id = 4,
                    Name = "Amira Youssef Mansour",
                    Email = "amira.youssef@outlook.com",
                    Phone = "+201112223334",
                    Gender = "Female",
                    DateOfBirth = new DateTime(2001, 8, 15),
                    Address = "12 El-Gesh St, Alexandria, Egypt",
                    MedicalHistory = "No major clinical history.",
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            );

            // 4. Appointments
            modelBuilder.Entity<Appointment>().HasData(
                new Appointment
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1, // Dr. Ahmed Hosni
                    AppointmentDate = DateTime.Today.AddDays(-5),
                    TimeSlot = new TimeSpan(10, 0, 0),
                    Status = "Completed",
                    Notes = "Routine cardiac checkup. Patient reported occasional chest tightness.",
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Appointment
                {
                    Id = 2,
                    PatientId = 2,
                    DoctorId = 2, // Dr. Sarah Jenkins
                    AppointmentDate = DateTime.Today.AddDays(-2),
                    TimeSlot = new TimeSpan(11, 30, 0),
                    Status = "Completed",
                    Notes = "General pediatric consult for child wellness check.",
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new Appointment
                {
                    Id = 3,
                    PatientId = 3,
                    DoctorId = 3, // Dr. Robert Chen
                    AppointmentDate = DateTime.Today.AddDays(1),
                    TimeSlot = new TimeSpan(14, 0, 0),
                    Status = "Approved",
                    Notes = "Post-op checkup for joints.",
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new Appointment
                {
                    Id = 4,
                    PatientId = 4,
                    DoctorId = 1, // Dr. Ahmed Hosni
                    AppointmentDate = DateTime.Today.AddDays(2),
                    TimeSlot = new TimeSpan(11, 0, 0),
                    Status = "Pending",
                    Notes = "Requesting consultation on cholesterol reports.",
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            );

            // 5. Medical Records (Linked to Completed Appointments)
            modelBuilder.Entity<MedicalRecord>().HasData(
                new MedicalRecord
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    Diagnosis = "Mild Hypertension & Sinus Bradycardia",
                    Treatment = "Adjust diet to limit sodium, recommend daily cardiovascular exercise (30 mins). Follow-up in 3 months.",
                    PrescribedMedications = "Lisinopril 10mg daily in the morning.",
                    Notes = "Patient ECG is stable. Re-evaluate blood pressure profile at next visit.",
                    RecordDate = DateTime.Today.AddDays(-5)
                },
                new MedicalRecord
                {
                    Id = 2,
                    PatientId = 2,
                    DoctorId = 2,
                    Diagnosis = "Healthy Child - Age 5 Wellness Exam",
                    Treatment = "Keep up with standard childhood immunization schedule. Balanced nutritional intake.",
                    PrescribedMedications = "Multivitamin syrup 5ml daily.",
                    Notes = "Growth chart parameters are in the 75th percentile. All reflexes normal.",
                    RecordDate = DateTime.Today.AddDays(-2)
                }
            );

            // 6. Bills
            modelBuilder.Entity<Bill>().HasData(
                new Bill
                {
                    Id = 1,
                    PatientId = 1,
                    AppointmentId = 1,
                    TotalAmount = 150.00m,
                    PaymentStatus = "Paid",
                    PaymentDate = DateTime.Today.AddDays(-5),
                    CreatedAt = DateTime.Today.AddDays(-5)
                },
                new Bill
                {
                    Id = 2,
                    PatientId = 2,
                    AppointmentId = 2,
                    TotalAmount = 80.00m,
                    PaymentStatus = "Paid",
                    PaymentDate = DateTime.Today.AddDays(-2),
                    CreatedAt = DateTime.Today.AddDays(-2)
                },
                new Bill
                {
                    Id = 3,
                    PatientId = 3,
                    AppointmentId = 3,
                    TotalAmount = 200.00m,
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Today.AddDays(-2)
                },
                new Bill
                {
                    Id = 4,
                    PatientId = 4,
                    AppointmentId = 4,
                    TotalAmount = 150.00m,
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.Today.AddDays(-1)
                }
            );
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;

namespace _4._Booking_AppointmentSystem.Models
{
    public class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<ProviderService> ProviderServices { get; set; }
        public DbSet<ProviderSchedule> ProviderSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for ProviderService
            modelBuilder.Entity<ProviderService>()
                .HasKey(ps => new { ps.ProviderId, ps.ServiceId });

            modelBuilder.Entity<ProviderService>()
                .HasOne(ps => ps.Provider)
                .WithMany(p => p.ProviderServices)
                .HasForeignKey(ps => ps.ProviderId);

            modelBuilder.Entity<ProviderService>()
                .HasOne(ps => ps.Service)
                .WithMany(s => s.ProviderServices)
                .HasForeignKey(ps => ps.ServiceId);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Healthcare & Clinics", Description = "Professional clinical and medical consultations", Icon = "heart-pulse" },
                new Category { Id = 2, Name = "Beauty Salons & Spa", Description = "Premium styling, cosmetics, and relaxation therapies", Icon = "scissors" },
                new Category { Id = 3, Name = "Business Consulting", Description = "Professional strategy meetings and career coaching", Icon = "briefcase" },
                new Category { Id = 4, Name = "Holistic Wellness", Description = "Therapeutic bodywork, yoga, and meditation sessions", Icon = "flower1" }
            );

            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                // Healthcare
                new Service { Id = 1, CategoryId = 1, Name = "General Physician Checkup", Description = "Comprehensive medical consultation and general checkup.", Price = 60.00m, DurationMinutes = 30 },
                new Service { Id = 2, CategoryId = 1, Name = "Professional Dental Cleaning", Description = "Plaque removal, teeth polishing, and deep hygiene assessment.", Price = 90.00m, DurationMinutes = 45 },
                new Service { Id = 3, CategoryId = 1, Name = "Cardiovascular Consultation", Description = "Advanced ECG test and cardiologist specialist evaluation.", Price = 180.00m, DurationMinutes = 60 },
                
                // Beauty & Salon
                new Service { Id = 4, CategoryId = 2, Name = "Signature Men's Haircut", Description = "Custom haircut, wash, styling, and hot towel finish.", Price = 35.00m, DurationMinutes = 30 },
                new Service { Id = 5, CategoryId = 2, Name = "Women's Balayage & Styling", Description = "High-end hair coloring, toning, deep conditioning, and blow dry.", Price = 150.00m, DurationMinutes = 120 },
                new Service { Id = 6, CategoryId = 2, Name = "HydraFacial Skin Treatment", Description = "Deep cleansing, exfoliation, and hydration infusion.", Price = 95.00m, DurationMinutes = 60 },

                // Business Consulting
                new Service { Id = 7, CategoryId = 3, Name = "Corporate Strategy Consult", Description = "1-on-1 strategic growth and scale planning session.", Price = 250.00m, DurationMinutes = 60 },
                new Service { Id = 8, CategoryId = 3, Name = "Executive Career Coaching", Description = "Resume audit, interview Prep, and leadership path coaching.", Price = 120.00m, DurationMinutes = 45 },

                // Holistic Wellness
                new Service { Id = 9, CategoryId = 4, Name = "Deep Tissue Muscle Massage", Description = "Focus on releasing chronic muscle tension and body knots.", Price = 85.00m, DurationMinutes = 60 },
                new Service { Id = 10, CategoryId = 4, Name = "Premium Hot Stone Therapy", Description = "Heated basalt stones massage to melt away stress and fatigue.", Price = 110.00m, DurationMinutes = 75 }
            );

            // Seed Providers
            modelBuilder.Entity<Provider>().HasData(
                new Provider { Id = 1, Name = "Dr. Sarah Jenkins", Title = "Senior Cardiologist", Bio = "Dr. Sarah holds a PhD in Cardiovascular Sciences from Johns Hopkins, boasting over 12 years of clinical research and practice.", Email = "dr.sarah@bookingsystem.com", Phone = "+1 555-0101", Rating = 4.9, ImagePath = "/images/provider-1.jpg" },
                new Provider { Id = 2, Name = "Dr. Ahmed Mansour", Title = "Specialist Cosmetic Dentist", Bio = "Dr. Ahmed specializes in advanced implantology and aesthetic smile makeovers with over 8 years in digital dentistry.", Email = "dr.ahmed@bookingsystem.com", Phone = "+1 555-0102", Rating = 4.8, ImagePath = "/images/provider-2.jpg" },
                new Provider { Id = 3, Name = "Maria Lopez", Title = "Master Hair Stylist & Colorist", Bio = "Maria is a certified master colorist, trained in Paris, renowned for creating personalized shades and modern haircuts.", Email = "maria.l@bookingsystem.com", Phone = "+1 555-0201", Rating = 4.7, ImagePath = "/images/provider-3.jpg" },
                new Provider { Id = 4, Name = "John Davis", Title = "Executive Life & Business Coach", Bio = "John is an ICF-certified coach helping professionals scale business startups and unlock their career potentials.", Email = "john.d@bookingsystem.com", Phone = "+1 555-0301", Rating = 5.0, ImagePath = "/images/provider-4.jpg" },
                new Provider { Id = 5, Name = "Yasmine El-Amin", Title = "Holistic Therapist & Masseur", Bio = "Yasmine has studied eastern relaxation therapies in Bali, specializing in trigger point release and hot stone body wellness.", Email = "yasmine.e@bookingsystem.com", Phone = "+1 555-0401", Rating = 4.9, ImagePath = "/images/provider-5.jpg" }
            );

            // Seed Provider-Service mapping
            modelBuilder.Entity<ProviderService>().HasData(
                // Dr. Sarah Jenkins -> Cardiovascular Consultation
                new ProviderService { ProviderId = 1, ServiceId = 3 },
                // Dr. Ahmed Mansour -> General Physician Checkup & Professional Dental Cleaning
                new ProviderService { ProviderId = 2, ServiceId = 1 },
                new ProviderService { ProviderId = 2, ServiceId = 2 },
                // Maria Lopez -> Signature Men's Haircut, Women's Balayage & Styling, HydraFacial
                new ProviderService { ProviderId = 3, ServiceId = 4 },
                new ProviderService { ProviderId = 3, ServiceId = 5 },
                new ProviderService { ProviderId = 3, ServiceId = 6 },
                // John Davis -> Corporate Strategy Consult, Executive Career Coaching
                new ProviderService { ProviderId = 4, ServiceId = 7 },
                new ProviderService { ProviderId = 4, ServiceId = 8 },
                // Yasmine El-Amin -> Deep Tissue Muscle Massage, Premium Hot Stone Therapy
                new ProviderService { ProviderId = 5, ServiceId = 9 },
                new ProviderService { ProviderId = 5, ServiceId = 10 }
            );

            // Seed Provider schedules
            // Let's seed schedules for Mon, Tue, Wed, Thu, Fri from 9 AM to 5 PM
            int scheduleId = 1;
            for (int providerId = 1; providerId <= 5; providerId++)
            {
                for (int day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
                {
                    modelBuilder.Entity<ProviderSchedule>().HasData(
                        new ProviderSchedule
                        {
                            Id = scheduleId++,
                            ProviderId = providerId,
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = new TimeSpan(9, 0, 0),  // 9:00 AM
                            EndTime = new TimeSpan(17, 0, 0),  // 5:00 PM
                            IsActive = true
                        }
                    );
                }
            }
        }
    }
}

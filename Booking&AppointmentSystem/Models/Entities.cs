using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _4._Booking_AppointmentSystem.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Icon { get; set; } = "grid"; // Bootstrap icon class name (e.g. "heart-pulse", "scissors", "mortarboard")

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }

    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(5, 480)]
        public int DurationMinutes { get; set; } // Duration in minutes

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    public class Provider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty; // e.g., "Senior Cardiologist", "Nail Stylist"

        [StringLength(1000)]
        public string Bio { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(250)]
        public string ImagePath { get; set; } = "/images/default-avatar.png";

        [Range(1.0, 5.0)]
        public double Rating { get; set; } = 5.0;

        public virtual ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
        public virtual ICollection<ProviderSchedule> Schedules { get; set; } = new List<ProviderSchedule>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

    public class ProviderService
    {
        public int ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }

        public int ServiceId { get; set; }
        public virtual Service? Service { get; set; }
    }

    public class ProviderSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public virtual Provider? Provider { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; } // Day of week (Sunday = 0, Monday = 1, etc.)

        [Required]
        public TimeSpan StartTime { get; set; } // e.g. 09:00:00

        [Required]
        public TimeSpan EndTime { get; set; } // e.g. 17:00:00

        public bool IsActive { get; set; } = true;
    }

    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(50)]
        public string CustomerPhone { get; set; } = string.Empty;

        [StringLength(500)]
        public string CustomerNotes { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string BookingCode { get; set; } = string.Empty; // Unique code (BK-XXXXXX) for tracking

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [ForeignKey("ProviderId")]
        public virtual Provider? Provider { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeSpan TimeSlot { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

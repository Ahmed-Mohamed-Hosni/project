using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        public virtual Doctor? Doctor { get; set; }

        [Required]
        [Display(Name = "Day of the Week")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(10, 120)]
        [Display(Name = "Slot Duration (Minutes)")]
        public int SlotDurationMinutes { get; set; } = 30;
    }
}

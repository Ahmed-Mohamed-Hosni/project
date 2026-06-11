using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class Bill
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        public virtual Patient? Patient { get; set; }

        [Required]
        [Display(Name = "Appointment")]
        public int AppointmentId { get; set; }

        public virtual Appointment? Appointment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        [Display(Name = "Total Amount ($)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Unpaid"; // Paid, Unpaid, PartiallyPaid

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        public virtual Patient? Patient { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        public virtual Doctor? Doctor { get; set; }

        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treatment plan is required")]
        [StringLength(1000)]
        public string Treatment { get; set; } = string.Empty;

        [Display(Name = "Prescribed Medications")]
        [StringLength(500)]
        public string? PrescribedMedications { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; } = DateTime.Now;
    }
}

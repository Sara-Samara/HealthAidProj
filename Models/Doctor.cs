// Models/Doctor.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Specialization is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Specialization must be between 3 and 50 characters.")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Years of experience is required.")]
        [Range(0, 60, ErrorMessage = "Years of experience must be between 0 and 60.")]
        public int YearsExperience { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "License number is required.")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "License number must be between 5 and 20 characters.")]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Available hours description cannot exceed 200 characters.")]
        public string? AvailableHours { get; set; }


        [Required(ErrorMessage = "User ID is required.")]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "User details are required.")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
        public virtual ICollection<MentalSupportSession> MentalSupportSessions { get; set; } = new List<MentalSupportSession>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
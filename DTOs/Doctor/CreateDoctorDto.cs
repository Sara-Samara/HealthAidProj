using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Doctors
{
    public class CreateDoctorDto
    {
        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Range(0, 60)]
        public int YearsExperience { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AvailableHours { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
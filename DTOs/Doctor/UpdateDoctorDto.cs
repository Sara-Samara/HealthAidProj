using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Doctors
{
    public class UpdateDoctorDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string? Specialization { get; set; }

        [Range(0, 60)]
        public int? YearsExperience { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(20, MinimumLength = 5)]
        public string? LicenseNumber { get; set; }

        [StringLength(200)]
        public string? AvailableHours { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicalFacilities
{
    public class CreateMedicalFacilityDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [StringLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public string Services { get; set; } = string.Empty;

        [StringLength(255)]
        public string OperatingHours { get; set; } = string.Empty;
    }
}
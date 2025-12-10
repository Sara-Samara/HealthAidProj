using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicalFacilities
{
    public class UpdateMedicalFacilityDto
    {

        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Services { get; set; }
        public string? OperatingHours { get; set; }
    }
}
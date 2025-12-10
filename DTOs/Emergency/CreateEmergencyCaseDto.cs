using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Emergency
{
    public class CreateEmergencyCaseDto
    {
        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Emergency type is required")]
        [StringLength(50, ErrorMessage = "Emergency type cannot exceed 50 characters")]
        public string EmergencyType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Priority must be Low, Medium, High, or Critical")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, ErrorMessage = "Location description cannot exceed 500 characters")]
        public string Location { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}
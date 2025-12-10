using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.PublicAlerts
{
    public class UpdatePublicAlertDto
    {
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string? Description { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Region must be between 3 and 100 characters")]
        public string? Region { get; set; }

        [StringLength(50, ErrorMessage = "Alert type cannot exceed 50 characters")]
        public string? AlertType { get; set; }

        [StringLength(20, ErrorMessage = "Severity cannot exceed 20 characters")]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Severity must be Low, Medium, High, or Critical")]
        public string? Severity { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "More info URL cannot exceed 500 characters")]
        public string? MoreInfoUrl { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
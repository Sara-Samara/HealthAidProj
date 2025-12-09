// DTOs/PublicAlertDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class PublicAlertDto
    {
        public int AlertId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public int UserId { get; set; }
        public string PostedBy { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public bool IsActive { get; set; } = true;
    }

    public class CreatePublicAlertDto
    {
        [Required(ErrorMessage = "Alert title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alert description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Region must be between 3 and 100 characters")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alert type is required")]
        [StringLength(50, ErrorMessage = "Alert type cannot exceed 50 characters")]
        public string AlertType { get; set; } = string.Empty;

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [StringLength(20, ErrorMessage = "Severity cannot exceed 20 characters")]
        [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Severity must be Low, Medium, High, or Critical")]
        public string Severity { get; set; } = "Medium";

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "More info URL cannot exceed 500 characters")]
        public string? MoreInfoUrl { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }

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

    public class PublicAlertFilterDto
    {
        public string? Region { get; set; }
        public string? AlertType { get; set; }
        public string? Severity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }

    public class AlertStatsDto
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsByRegion { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
        public int TodayAlerts { get; set; }
    }
}
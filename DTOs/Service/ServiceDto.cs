// DTOs/ServiceDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ProviderId { get; set; }
        public string? ProviderType { get; set; }
        public string? ProviderName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsFree => !Price.HasValue || Price == 0;
    }

    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Service name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression("^(Medical|Donation|Equipment|Consultation|MentalHealth|Emergency|Education|Other)$",
            ErrorMessage = "Invalid category")]
        public string Category { get; set; } = "Medical";

        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Active|Inactive|Pending)$", ErrorMessage = "Status must be Active, Inactive, or Pending")]
        public string Status { get; set; } = "Active";

        public int? ProviderId { get; set; }

        [StringLength(20, ErrorMessage = "Provider type cannot exceed 20 characters")]
        [RegularExpression("^(Doctor|NGO|System|Hospital|Clinic)$", ErrorMessage = "Invalid provider type")]
        public string? ProviderType { get; set; }
    }

    public class UpdateServiceDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Service name must be between 3 and 100 characters")]
        public string? Name { get; set; }

        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression("^(Medical|Donation|Equipment|Consultation|MentalHealth|Emergency|Education|Other)$",
            ErrorMessage = "Invalid category")]
        public string? Category { get; set; }

        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000")]
        public decimal? Price { get; set; }

        [RegularExpression("^(Active|Inactive|Pending)$", ErrorMessage = "Status must be Active, Inactive, or Pending")]
        public string? Status { get; set; }

        public int? ProviderId { get; set; }

        [StringLength(20, ErrorMessage = "Provider type cannot exceed 20 characters")]
        [RegularExpression("^(Doctor|NGO|System|Hospital|Clinic)$", ErrorMessage = "Invalid provider type")]
        public string? ProviderType { get; set; }
    }

    public class ServiceFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? ProviderType { get; set; }
        public int? ProviderId { get; set; }
        public bool? IsFree { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? UpdatedAfter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class ServiceStatsDto
    {
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int FreeServices { get; set; }
        public int PaidServices { get; set; }
        public Dictionary<string, int> CategoryCount { get; set; } = new();
        public Dictionary<string, int> ProviderTypeCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public decimal AveragePrice { get; set; }
        public int RecentServices { get; set; }
    }

    public class AssignServiceProviderDto
    {
        [Required(ErrorMessage = "Provider ID is required")]
        public int ProviderId { get; set; }

        [Required(ErrorMessage = "Provider type is required")]
        [StringLength(20, ErrorMessage = "Provider type cannot exceed 20 characters")]
        [RegularExpression("^(Doctor|NGO|System|Hospital|Clinic)$", ErrorMessage = "Invalid provider type")]
        public string ProviderType { get; set; } = string.Empty;
    }
}
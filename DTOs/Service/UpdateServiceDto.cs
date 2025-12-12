using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Services
{
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
}
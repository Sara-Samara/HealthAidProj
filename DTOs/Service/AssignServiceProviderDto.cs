using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Services
{
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
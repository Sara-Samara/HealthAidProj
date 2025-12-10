using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Sponsorships
{
    public class CreateSponsorshipDto
    {
        [Required(ErrorMessage = "Goal description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Goal description must be between 10 and 500 characters")]
        public string GoalDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Goal amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Goal amount must be greater than 0")]
        public decimal GoalAmount { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression("^(Medical|Education|Shelter|Food|Other)$", ErrorMessage = "Invalid category")]
        public string Category { get; set; } = "Medical";

        [StringLength(1000, ErrorMessage = "Story cannot exceed 1000 characters")]
        public string? Story { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        public DateTime? Deadline { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }
    }
}
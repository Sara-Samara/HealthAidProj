using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Sponsorships
{
    public class UpdateSponsorshipDto
    {
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Goal description must be between 10 and 500 characters")]
        public string? GoalDescription { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Goal amount must be greater than 0")]
        public decimal? GoalAmount { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Active|Completed|Cancelled|Paused)$", ErrorMessage = "Invalid status")]
        public string? Status { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression("^(Medical|Education|Shelter|Food|Other)$", ErrorMessage = "Invalid category")]
        public string? Category { get; set; }

        [StringLength(1000, ErrorMessage = "Story cannot exceed 1000 characters")]
        public string? Story { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        public DateTime? Deadline { get; set; }
    }
}
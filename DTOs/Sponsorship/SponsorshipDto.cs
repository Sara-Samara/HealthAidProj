// DTOs/SponsorshipDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class SponsorshipDto
    {
        public int SponsorshipId { get; set; }
        public string GoalDescription { get; set; } = string.Empty;
        public decimal GoalAmount { get; set; }
        public decimal AmountRaised { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Story { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? Deadline { get; set; }
        public int DonorCount { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsFullyFunded { get; set; }
        public bool IsUrgent { get; set; }
        public decimal AmountNeeded { get; set; }
    }

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

    public class SponsorshipFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public int? PatientId { get; set; }
        public bool? IsUrgent { get; set; }
        public bool? IsFullyFunded { get; set; }
        public decimal? MinGoalAmount { get; set; }
        public decimal? MaxGoalAmount { get; set; }
        public DateTime? DeadlineBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class DonateToSponsorshipDto
    {
        [Required(ErrorMessage = "Donor ID is required")]
        public int DonorId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string? Message { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string PaymentMethod { get; set; } = "Credit Card";
    }

    public class SponsorshipStatsDto
    {
        public int TotalSponsorships { get; set; }
        public int ActiveSponsorships { get; set; }
        public int CompletedSponsorships { get; set; }
        public decimal TotalGoalAmount { get; set; }
        public decimal TotalAmountRaised { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalDonors { get; set; }
        public Dictionary<string, int> CategoryCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public int UrgentSponsorships { get; set; }
    }
}
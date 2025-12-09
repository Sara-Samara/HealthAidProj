using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class Sponsorship
    {
        public int SponsorshipId { get; set; }

        [Required(ErrorMessage = "Goal description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Goal description must be between 10 and 500 characters")]
        public string GoalDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Goal amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Goal amount must be greater than 0")]
        public decimal GoalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount raised must be a positive number")]
        public decimal AmountRaised { get; set; } = 0;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Active|Completed|Cancelled|Paused)$", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "Active";

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
        public int DonorCount { get; set; } = 0;


        [Required(ErrorMessage = "Patient ID is required")]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [JsonIgnore]
        public virtual Patient Patient { get; set; } = null!;

        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public decimal ProgressPercentage => GoalAmount > 0 ? (AmountRaised / GoalAmount) * 100 : 0;
        public bool IsFullyFunded => AmountRaised >= GoalAmount;
        public bool IsUrgent => Deadline.HasValue && (Deadline.Value - DateTime.UtcNow).Days <= 7;
        public decimal AmountNeeded => GoalAmount - AmountRaised;
    }
}
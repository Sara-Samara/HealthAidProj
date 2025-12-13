using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Donations
{
    public class UpdateDonationStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Completed|Failed|Refunded)$", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TransactionId { get; set; }
    }
}
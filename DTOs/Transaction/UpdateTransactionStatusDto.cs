using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Transactions
{
    public class UpdateTransactionStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string Status { get; set; } = string.Empty;
    }
}
using HealthAidAPI.DTOs.Messages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Transactions
{
    public class CreateTransactionDto
    {
        [Required]
        [Range(0.01, 1000000.00)]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        public DateTime? Date { get; set; }

        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int? UserId { get; set; }
    }
}
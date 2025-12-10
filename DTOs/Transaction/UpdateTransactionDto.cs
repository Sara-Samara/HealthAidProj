using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Transactions
{
    public class UpdateTransactionDto
    {
        [Range(0.01, 1000000.00)]
        public decimal? Amount { get; set; }

        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string? Status { get; set; }

        public DateTime? Date { get; set; }

        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public int? UserId { get; set; }
    }
}
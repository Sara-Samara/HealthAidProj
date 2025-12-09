using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Transaction
    {
        [Key] 
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Transaction amount is required.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, 1000000.00, ErrorMessage = "Amount must be between 0.01 and 1,000,000.")]
        [Display(Name = "Transaction Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$",
            ErrorMessage = "Status must be Pending, Completed, Failed, Refunded, or Cancelled.")]
        [Display(Name = "Transaction Status")]
        public string Status { get; set; } = "Pending"; 

        [Required(ErrorMessage = "Transaction date is required.")]
        [DataType(DataType.DateTime)] 
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [Display(Name = "Transaction Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int? ConsultationId { get; set; }
        public virtual Consultation? Consultation { get; set; }

        public int? DonationId { get; set; }
        public virtual Donation? Donation { get; set; }

        public int? MedicineRequestId { get; set; }
        public virtual MedicineRequest? MedicineRequest { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
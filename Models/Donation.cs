using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        [NotMapped] 
        public string DonorName => IsAnonymous ? "Anonymous" :
                                 Donor?.User != null ?
                                 $"{Donor.User.FirstName} {Donor.User.LastName}" :
                                 Donor?.Organization ?? "Unknown Donor";

        [Required(ErrorMessage = "Donation amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        [RegularExpression("^(CreditCard|BankTransfer|Cash|MobilePayment|Other)$", ErrorMessage = "Invalid payment method")]
        public string PaymentMethod { get; set; } = "Other";

        [Required(ErrorMessage = "Donation status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Pending|Completed|Failed|Refunded)$", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "Pending";

        [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
        public string? TransactionId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Donation type is required")]
        [StringLength(50, ErrorMessage = "Donation type cannot exceed 50 characters")]
        [RegularExpression("^(OneTime|Monthly|Quarterly|Annual)$", ErrorMessage = "Invalid donation type")]
        public string DonationType { get; set; } = "OneTime";

        public bool IsAnonymous { get; set; } = false;
        public bool ReceiveUpdates { get; set; } = true;

        [Required(ErrorMessage = "Sponsorship ID is required")]
        [ForeignKey("Sponsorship")]
        public int SponsorshipId { get; set; }

        [JsonIgnore]
        public virtual Sponsorship? Sponsorship { get; set; }

        [ForeignKey("Donor")]
        public int? DonorId { get; set; }

        [JsonIgnore]
        public virtual Donor? Donor { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public DateTime DateDonated { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }

        public bool IsProcessed => Status == "Completed" && ProcessedDate.HasValue;
        public bool IsRecent => DateDonated >= DateTime.UtcNow.AddDays(-7);

        public string? Message { get; internal set; }
    }
}
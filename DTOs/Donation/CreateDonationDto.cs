using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Donations
{
    public class CreateDonationDto
    {
        [Required(ErrorMessage = "Donation amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50)]
        [RegularExpression("^(CreditCard|BankTransfer|Cash|MobilePayment|Other)$", ErrorMessage = "Invalid payment method")]
        public string PaymentMethod { get; set; } = "CreditCard";

        [Required(ErrorMessage = "Donation type is required")]
        [StringLength(50)]
        [RegularExpression("^(OneTime|Monthly|Quarterly|Annual)$")]
        public string DonationType { get; set; } = "OneTime";

        [Required(ErrorMessage = "Sponsorship ID is required")]
        public int SponsorshipId { get; set; }

        public int? DonorId { get; set; } // يمكن أن يكون null للمستخدمين غير المسجلين كـ Donor رسمي

        public bool IsAnonymous { get; set; } = false;
        public bool ReceiveUpdates { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }
    }
}
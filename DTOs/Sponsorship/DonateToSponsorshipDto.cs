using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Sponsorships
{
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
}
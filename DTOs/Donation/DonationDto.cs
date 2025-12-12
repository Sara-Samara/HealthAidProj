namespace HealthAidAPI.DTOs.Donations
{
    public class DonationDto
    {
        public int DonationId { get; set; }
        public decimal Amount { get; set; }
        public string DonorName { get; set; } = string.Empty; // الاسم المحسوب
        public string? DonorEmail { get; set; }
        public int? DonorId { get; set; }
        public int SponsorshipId { get; set; }
        public string SponsorshipTitle { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DonationType { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string? Message { get; set; }
        public string? TransactionId { get; set; }
    }
}
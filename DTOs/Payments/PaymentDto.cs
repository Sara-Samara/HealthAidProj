namespace HealthAidAPI.DTOs.Payments
{
    public class PaymentRequestDto
    {
        public int DonationId { get; set; } 
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class PaymentResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}
namespace HealthAidAPI.DTOs.Donations
{
    public class DonationDto
    {
        public int DonationId { get; set; }
        public decimal Amount { get; set; }
        public string? DonorName { get; set; }
        // Add other donation properties as needed
    }
}
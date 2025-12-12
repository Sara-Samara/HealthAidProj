namespace HealthAidAPI.DTOs.Donations
{
    public class DonationStatsDto
    {
        public int TotalDonations { get; set; }
        public decimal TotalAmountRaised { get; set; }
        public int CompletedDonationsCount { get; set; }
        public int PendingDonationsCount { get; set; }
        public Dictionary<string, decimal> DonationsByMethod { get; set; } = new();
        public decimal AverageDonationAmount { get; set; }
    }
}
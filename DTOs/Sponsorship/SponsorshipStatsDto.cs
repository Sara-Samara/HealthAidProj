using System.Collections.Generic;

namespace HealthAidAPI.DTOs.Sponsorships
{
    public class SponsorshipStatsDto
    {
        public int TotalSponsorships { get; set; }
        public int ActiveSponsorships { get; set; }
        public int CompletedSponsorships { get; set; }
        public decimal TotalGoalAmount { get; set; }
        public decimal TotalAmountRaised { get; set; }
        public decimal TotalDonations { get; set; }
        public int TotalDonors { get; set; }
        public Dictionary<string, int> CategoryCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public int UrgentSponsorships { get; set; }
    }
}
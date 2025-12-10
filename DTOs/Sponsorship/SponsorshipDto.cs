namespace HealthAidAPI.DTOs.Sponsorships
{
    public class SponsorshipDto
    {
        public int SponsorshipId { get; set; }
        public string GoalDescription { get; set; } = string.Empty;
        public decimal GoalAmount { get; set; }
        public decimal AmountRaised { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Story { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? Deadline { get; set; }
        public int DonorCount { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal ProgressPercentage { get; set; }
        public bool IsFullyFunded { get; set; }
        public bool IsUrgent { get; set; }
        public decimal AmountNeeded { get; set; }
    }
}
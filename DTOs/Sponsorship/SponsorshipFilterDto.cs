namespace HealthAidAPI.DTOs.Sponsorships
{
    public class SponsorshipFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public int? PatientId { get; set; }
        public bool? IsUrgent { get; set; }
        public bool? IsFullyFunded { get; set; }
        public decimal? MinGoalAmount { get; set; }
        public decimal? MaxGoalAmount { get; set; }
        public DateTime? DeadlineBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
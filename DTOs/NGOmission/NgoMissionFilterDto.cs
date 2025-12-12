namespace HealthAidAPI.DTOs.NgoMissions
{
    public class NgoMissionFilterDto
    {
        public string? Search { get; set; }
        public string? Location { get; set; }
        public int? NGOId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string? Status { get; set; } // Upcoming, Ongoing, Completed

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
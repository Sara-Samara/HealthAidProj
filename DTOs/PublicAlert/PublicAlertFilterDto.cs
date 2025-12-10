namespace HealthAidAPI.DTOs.PublicAlerts
{
    public class PublicAlertFilterDto
    {
        public string? Region { get; set; }
        public string? AlertType { get; set; }
        public string? Severity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }
}
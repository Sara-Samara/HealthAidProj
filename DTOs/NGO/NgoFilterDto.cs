namespace HealthAidAPI.DTOs.NGOs
{
    public class NgoFilterDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? AreaOfWork { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
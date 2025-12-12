namespace HealthAidAPI.DTOs.HealthGuides
{
    public class HealthGuideFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Language { get; set; }
        public bool? IsPublished { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
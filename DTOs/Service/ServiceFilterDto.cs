namespace HealthAidAPI.DTOs.Services
{
    public class ServiceFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? ProviderType { get; set; }
        public int? ProviderId { get; set; }
        public bool? IsFree { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? UpdatedAfter { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
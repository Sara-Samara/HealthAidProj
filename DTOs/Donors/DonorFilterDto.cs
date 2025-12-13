namespace HealthAidAPI.DTOs.Donors
{
    public class DonorFilterDto
    {
        public string? Search { get; set; } // بحث بالاسم أو المنظمة
        public decimal? MinTotalDonated { get; set; }
        public decimal? MaxTotalDonated { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
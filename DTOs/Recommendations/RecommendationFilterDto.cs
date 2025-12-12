namespace HealthAidAPI.DTOs.Recommendations
{
    public class RecommendationFilterDto
    {
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? Priority { get; set; }
        public bool? IsViewed { get; set; }
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
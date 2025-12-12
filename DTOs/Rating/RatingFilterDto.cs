namespace HealthAidAPI.DTOs.Ratings
{
    public class RatingFilterDto
    {
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public int? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }
}
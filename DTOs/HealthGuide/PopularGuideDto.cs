namespace HealthAidAPI.DTOs.HealthGuides
{
    public class PopularGuideDto
    {
        public int HealthGuideId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
    }
}
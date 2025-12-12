namespace HealthAidAPI.DTOs.HealthGuides
{
    public class HealthGuideDto
    {
        public int HealthGuideId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string? Summary { get; set; }
        public int? UserId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = true;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int ReadingTime { get; set; }
        public string TruncatedContent { get; set; } = string.Empty;
    }
}
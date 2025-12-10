namespace HealthAidAPI.DTOs.Ratings
{
    public class RatingDto
    {
        public int RatingId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
    }
}
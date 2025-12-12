using System.Collections.Generic;

namespace HealthAidAPI.DTOs.Ratings
{
    public class AverageRatingDto
    {
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public double Average { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<int, int> Distribution { get; set; } = new();
        public string TargetName { get; set; } = string.Empty;
    }
}
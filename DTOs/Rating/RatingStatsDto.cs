using System.Collections.Generic;

namespace HealthAidAPI.DTOs.Ratings
{
    public class RatingStatsDto
    {
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int FiveStarRatings { get; set; }
        public int OneStarRatings { get; set; }
        public int RecentRatings { get; set; }
    }
}
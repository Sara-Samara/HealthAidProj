using System.Collections.Generic; // ضروري عشان الـ Dictionary و List

namespace HealthAidAPI.DTOs.HealthGuides
{
    public class HealthGuideStatsDto
    {
        public int TotalGuides { get; set; }
        public int PublishedGuides { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public Dictionary<string, int> CategoriesCount { get; set; } = new();
        public Dictionary<string, int> LanguagesCount { get; set; } = new();

        // يعتمد على الكلاس الثاني بالأسفل
        public List<PopularGuideDto> PopularGuides { get; set; } = new();
    }
}
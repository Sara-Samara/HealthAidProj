using System.Collections.Generic;

namespace HealthAidAPI.DTOs.Services
{
    public class ServiceStatsDto
    {
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int FreeServices { get; set; }
        public int PaidServices { get; set; }
        public Dictionary<string, int> CategoryCount { get; set; } = new();
        public Dictionary<string, int> ProviderTypeCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public decimal AveragePrice { get; set; }
        public int RecentServices { get; set; }
    }
}
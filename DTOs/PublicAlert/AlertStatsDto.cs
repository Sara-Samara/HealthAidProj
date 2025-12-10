using System.Collections.Generic;

namespace HealthAidAPI.DTOs.PublicAlerts
{
    public class AlertStatsDto
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> AlertsByRegion { get; set; } = new();
        public Dictionary<string, int> AlertsBySeverity { get; set; } = new();
        public int TodayAlerts { get; set; }
    }
}
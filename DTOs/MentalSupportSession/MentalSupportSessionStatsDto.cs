namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class MentalSupportSessionStatsDto
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public Dictionary<string, int> SessionTypesCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public List<RecentSessionDto> RecentSessions { get; set; } = new();
    }
}
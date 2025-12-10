namespace HealthAidAPI.DTOs.NgoMissions
{
    public class MissionStatsDto
    {
        public int TotalMissions { get; set; }
        public int UpcomingMissions { get; set; }
        public int OngoingMissions { get; set; }
        public int CompletedMissions { get; set; }
        public Dictionary<string, int> MissionsByLocation { get; set; } = new();
        public Dictionary<int, int> MissionsByNGO { get; set; } = new();
    }
}
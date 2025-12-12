namespace HealthAidAPI.DTOs.NgoMissions
{
    public class NgoMissionDto
    {
        public int NgoMissionId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int NGOId { get; set; }
        public string NGOName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
    }
}
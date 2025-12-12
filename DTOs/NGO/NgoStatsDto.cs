namespace HealthAidAPI.DTOs.NGOs
{
    public class NgoStatsDto
    {
        public int TotalNgos { get; set; }
        public int VerifiedNgos { get; set; }
        public int PendingNgos { get; set; }
        public int RejectedNgos { get; set; }
        public Dictionary<string, int> NgosByArea { get; set; } = new();
        public int TotalMissions { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalPatients { get; set; }
    }
}
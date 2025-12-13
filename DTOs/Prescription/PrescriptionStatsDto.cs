namespace HealthAidAPI.DTOs.Prescriptions
{
    public class PrescriptionStatsDto
    {
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int CompletedPrescriptions { get; set; }
        public int ExpiredPrescriptions { get; set; }
        public Dictionary<string, int> MedicineFrequency { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }

        public List<RecentPrescriptionDto> RecentPrescriptions { get; set; } = new();
    }
}
namespace HealthAidAPI.DTOs.Consultations
{
    public class ConsultationStatsDto
    {
        public int TotalConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int PendingConsultations { get; set; }
        public int CanceledConsultations { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> ModeDistribution { get; set; } = new();
        public int ConsultationsThisMonth { get; set; }
        public double AverageConsultationsPerDay { get; set; }
    }
}
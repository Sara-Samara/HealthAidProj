namespace HealthAidAPI.Models.Extras
{
    public class SecondOpinion
    {
        public int Id { get; set; }
        public int OriginalConsultationId { get; set; } 
        public int RequestingDoctorId { get; set; }
        public int? ExpertDoctorId { get; set; }
        public string CaseSummary { get; set; } = string.Empty;
        public string? ExpertOpinion { get; set; }
        public string Status { get; set; } = "Pending"; 
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}
namespace HealthAidAPI.Models.Extras
{
    public class Report
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? TargetEntity { get; set; } 
        public bool IsAnonymous { get; set; } = false;
        public int? ReporterId { get; set; }
        public string Status { get; set; } = "Submitted";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
public class ConsultationAnalyticsDto
{
    public int TotalConsultations { get; set; }
    public int CompletedConsultations { get; set; }
    public int CancelledConsultations { get; set; }
    public int PendingConsultations { get; set; }
    public double AverageRating { get; set; }
    public decimal TotalRevenue { get; set; }
}
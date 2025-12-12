namespace HealthAidAPI.DTOs.MedicineRequests
{
    public class MedicineRequestStatsDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int UrgentRequests { get; set; }
        public int EmergencyRequests { get; set; }
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public Dictionary<string, int> PriorityCount { get; set; } = new();
        public int AverageDaysPending { get; set; }
        public int OverdueRequests { get; set; }
    }
}
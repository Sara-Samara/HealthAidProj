namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class RecentSessionDto
    {
        public int MentalSupportSessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
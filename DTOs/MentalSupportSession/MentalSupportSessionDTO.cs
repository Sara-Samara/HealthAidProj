namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class MentalSupportSessionDto
    {
        public int MentalSupportSessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientEmail { get; set; }
        public string? DoctorEmail { get; set; }
        public string Status { get; set; } = "Scheduled";
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
namespace HealthAidAPI.DTOs.Appointments
{
    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }
}
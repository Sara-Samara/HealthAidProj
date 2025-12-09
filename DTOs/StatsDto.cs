// DTOs/StatsDto.cs
namespace HealthAidAPI.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int Patients { get; set; }
        public int Doctors { get; set; }
        public int Donors { get; set; }
        public int RecentUsers { get; set; }
    }

    public class DailyRegistrationDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
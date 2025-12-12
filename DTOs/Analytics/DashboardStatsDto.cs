using HealthAidAPI.DTOs.Donations; // افترضنا أن لديك DonationDto

namespace HealthAidAPI.DTOs.Analytics
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalConsultations { get; set; }
        public int TotalDonations { get; set; }
        public int TotalEmergencyCases { get; set; }
        public int TotalMedicalFacilities { get; set; }

        // استخدام DTO بدلاً من Model
        public List<DonationDto> RecentDonations { get; set; } = new();
    }
}
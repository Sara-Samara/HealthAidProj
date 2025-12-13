using HealthAidAPI.DTOs.Donations;

namespace HealthAidAPI.DTOs.Analytics
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalDonors { get; set; }
        public int TotalConsultations { get; set; }
        public int ActiveConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int TotalDonations { get; set; }
        public decimal TotalDonationsAmount { get; set; }
        public int TotalEmergencyCases { get; set; }
        public int ActiveEmergencyCases { get; set; }
        public int TotalMedicalFacilities { get; set; }
        public int ActiveFacilities { get; set; }
        public List<DonationDto> RecentDonations { get; set; } = new();
        public int NewUsersToday { get; set; }
        public int ConsultationsToday { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
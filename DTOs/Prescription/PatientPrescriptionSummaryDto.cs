namespace HealthAidAPI.DTOs.Prescriptions
{
    public class PatientPrescriptionSummaryDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }

        public List<PrescriptionDto> RecentPrescriptions { get; set; } = new();
    }
}
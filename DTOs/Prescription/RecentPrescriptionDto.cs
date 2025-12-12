namespace HealthAidAPI.DTOs.Prescriptions
{
    public class RecentPrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
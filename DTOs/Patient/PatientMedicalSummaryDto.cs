namespace HealthAidAPI.DTOs.Patients
{
    public class PatientMedicalSummaryDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? BloodType { get; set; }
        public bool HasMedicalHistory { get; set; }
        public int TotalConsultations { get; set; }
        public int TotalMedicineRequests { get; set; }
        public DateTime? LastConsultationDate { get; set; }
        public bool HasChronicConditions { get; set; }
    }
}
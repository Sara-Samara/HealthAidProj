namespace HealthAidAPI.DTOs.Recommendations
{
    public class PatientHealthProfileDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string ChronicDiseases { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string FamilyMedicalHistory { get; set; } = string.Empty;
        public string Lifestyle { get; set; } = string.Empty;
        public bool Smoking { get; set; }
        public bool Alcohol { get; set; }
        public DateTime? LastCheckup { get; set; }
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
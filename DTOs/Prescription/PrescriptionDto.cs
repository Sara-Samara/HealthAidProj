namespace HealthAidAPI.DTOs.Prescriptions
{
    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosages { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int ConsultationId { get; set; }
        public string? ConsultationNotes { get; set; }
        public DateTime? ConsultationDate { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public string? Instructions { get; set; }
        public int? RefillsRemaining { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
using HealthAidAPI.DTOs.Doctors;
using HealthAidAPI.DTOs.Patients;
namespace HealthAidAPI.DTOs.Consultations
{
    public class ConsultationDto
    {
        public int ConsultationId { get; set; }
        public DateTime? ConsDate { get; set; }
        public string? Diagnosis { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Mode { get; set; }
        public string? Note { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? AppointmentId { get; set; }

        public DoctorDto? Doctor { get; set; }
        public PatientDto? Patient { get; set; }



        public int PrescriptionCount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

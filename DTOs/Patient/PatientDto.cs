using HealthAidAPI.DTOs.NGOs;
using HealthAidAPI.DTOs.Users;

namespace HealthAidAPI.DTOs.Patients
{
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? MedicalHistory { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? BloodType { get; set; }
        public int? UserId { get; set; }
        public int? NGOId { get; set; }

        public UserDto? User { get; set; }
        public NGODto? NGO { get; set; }

        public int TotalConsultations { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalMedicineRequests { get; set; }
        public int? Age { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
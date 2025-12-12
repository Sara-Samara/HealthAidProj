using HealthAidAPI.DTOs.Users;

namespace HealthAidAPI.DTOs.Doctors
{
    public class DoctorDto
    {
        public int DoctorId { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public int YearsExperience { get; set; }
        public string? Bio { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string? AvailableHours { get; set; }
        public int UserId { get; set; }

        public UserDto? User { get; set; }

        public int TotalConsultations { get; set; }
        public double? AverageRating { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
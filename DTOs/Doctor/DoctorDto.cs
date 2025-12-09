// DTOs/DoctorDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
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

    public class CreateDoctorDto
    {
        [Required]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [Range(0, 60)]
        public int YearsExperience { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AvailableHours { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class UpdateDoctorDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string? Specialization { get; set; }

        [Range(0, 60)]
        public int? YearsExperience { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(20, MinimumLength = 5)]
        public string? LicenseNumber { get; set; }

        [StringLength(200)]
        public string? AvailableHours { get; set; }

        public bool? IsAvailable { get; set; }
    }

    public class DoctorFilterDto
    {
        public string? Specialization { get; set; }
        public string? Search { get; set; }
        public int? MinYearsExperience { get; set; }
        public int? MaxYearsExperience { get; set; }
        public bool? IsAvailable { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class DoctorStatsDto
    {
        public int TotalDoctors { get; set; }
        public int AvailableDoctors { get; set; }
        public Dictionary<string, int> SpecializationsCount { get; set; } = new();
        public double AverageExperience { get; set; }
        public int NewDoctorsThisMonth { get; set; }
    }
}
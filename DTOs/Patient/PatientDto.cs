using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
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
        public int? NGOId { get; set; } // تم التصحيح من NgosId إلى NGOId
        public UserDto? User { get; set; }
        public NGODto? NGO { get; set; }
        public int TotalConsultations { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalMedicineRequests { get; set; }
        public int? Age { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreatePatientDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string PatientName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [RegularExpression("^(Male|Female)$")]
        public string? Gender { get; set; }

        [RegularExpression("^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$")]
        public string? BloodType { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? NGOId { get; set; } // تم التصحيح من NgosId إلى NGOId
    }

    public class UpdatePatientDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string? PatientName { get; set; }

        [StringLength(1000)]
        public string? MedicalHistory { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [RegularExpression("^(Male|Female)$")]
        public string? Gender { get; set; }

        [RegularExpression("^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$")]
        public string? BloodType { get; set; }

        public int? NGOId { get; set; } // تم التصحيح من NgosId إلى NGOId
    }

    public class PatientFilterDto
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public string? BloodType { get; set; }
        public int? NGOId { get; set; } // تم التصحيح من NgosId إلى NGOId
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool? HasMedicalHistory { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class PatientStatsDto
    {
        public int TotalPatients { get; set; }
        public Dictionary<string, int> GenderDistribution { get; set; } = new();
        public Dictionary<string, int> BloodTypeDistribution { get; set; } = new();
        public double AverageAge { get; set; }
        public int PatientsWithMedicalHistory { get; set; }
        public int NewPatientsThisMonth { get; set; }
    }

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

    public class NGODto
    {
        public int NGOId { get; set; } // تم التصحيح من NgosId إلى NGOId
        public string OrganizationName { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
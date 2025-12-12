using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Patients
{
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

        public int? NGOId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Recommendations
{
    public class CreateHealthProfileDto
    {
        [Required]
        public int PatientId { get; set; }

        [StringLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        public string ChronicDiseases { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string FamilyMedicalHistory { get; set; } = string.Empty;

        [StringLength(50)]
        public string Lifestyle { get; set; } = string.Empty;

        public bool Smoking { get; set; }
        public bool Alcohol { get; set; }
        public DateTime? LastCheckup { get; set; }

        [StringLength(255)]
        public string EmergencyContactName { get; set; } = string.Empty;

        [StringLength(20)]
        public string EmergencyContactNumber { get; set; } = string.Empty;
    }
}
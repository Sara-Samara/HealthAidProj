using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Recommendations
{
    public class UpdateHealthProfileDto
    {
        [StringLength(5)]
        public string? BloodType { get; set; }

        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        public string? ChronicDiseases { get; set; }
        public string? Allergies { get; set; }
        public string? Medications { get; set; }
        public string? FamilyMedicalHistory { get; set; }

        [StringLength(50)]
        public string? Lifestyle { get; set; }

        public bool? Smoking { get; set; }
        public bool? Alcohol { get; set; }
        public DateTime? LastCheckup { get; set; }

        [StringLength(255)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactNumber { get; set; }
    }
}
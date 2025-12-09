// Models/Recommendations/PatientHealthProfile.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Recommendations
{
    [Table("PatientHealthProfiles")]
    public class PatientHealthProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [StringLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 2)")]
        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        [Range(0, 500)]
        public decimal? Weight { get; set; }

        public string ChronicDiseases { get; set; } = string.Empty;

        public string Allergies { get; set; } = string.Empty;

        public string Medications { get; set; } = string.Empty;

        public string FamilyMedicalHistory { get; set; } = string.Empty;

        [StringLength(50)]
        public string Lifestyle { get; set; } = string.Empty;

        public bool Smoking { get; set; } = false;

        public bool Alcohol { get; set; } = false;

        [Column(TypeName = "date")]
        public DateTime? LastCheckup { get; set; }

        [StringLength(255)]
        public string EmergencyContactName { get; set; } = string.Empty;

        [StringLength(20)]
        public string EmergencyContactNumber { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}
// Models/Recommendations/DoctorRecommendation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Recommendations
{
    [Table("DoctorRecommendations")]
    public class DoctorRecommendation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [StringLength(50)]
        public string RecommendationType { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 4)")]
        [Range(0.0000, 1.0000)]
        public decimal MatchScore { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        public bool IsViewed { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
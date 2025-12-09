// Models/Emergency/EmergencyCase.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Emergency
{
    [Table("EmergencyCases")]
    public class EmergencyCase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmergencyType { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public string Description { get; set; } = string.Empty;

        public int? AssignedDoctorId { get; set; }

        public int? ResponderId { get; set; }

        public string ResolutionNotes { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("AssignedDoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("ResponderId")]
        public virtual EmergencyResponder? Responder { get; set; }

        public virtual ICollection<EmergencyLog> EmergencyLogs { get; set; } = new List<EmergencyLog>();
    }
}
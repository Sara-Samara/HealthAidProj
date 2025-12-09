// Models/Emergency/EmergencyLog.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Emergency
{
    [Table("EmergencyLogs")]
    public class EmergencyLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int EmergencyCaseId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        public int PerformedBy { get; set; }

        public string Notes { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EmergencyCaseId")]
        public virtual EmergencyCase EmergencyCase { get; set; } = null!;

        [ForeignKey("PerformedBy")]
        public virtual User User { get; set; } = null!;
    }
}
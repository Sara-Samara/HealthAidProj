// Models/Emergency/EmergencyResponder.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Emergency
{
    [Table("EmergencyResponders")]
    public class EmergencyResponder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal ResponseRadius { get; set; } = 10.00m;

        [StringLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        public string Qualifications { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3, 2)")]
        [Range(0.00, 5.00)]
        public decimal Rating { get; set; } = 0.00m;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<EmergencyCase> EmergencyCases { get; set; } = new List<EmergencyCase>();
    }
}
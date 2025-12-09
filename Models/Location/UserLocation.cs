// Models/Location/UserLocation.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Location
{
    [Table("UserLocations")]
    public class UserLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 8)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(11, 8)")]
        public decimal Longitude { get; set; }

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal? Accuracy { get; set; }

        [StringLength(50)]
        public string LocationType { get; set; } = "Current";

        public bool IsPrimary { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
// Models/Location/ServiceArea.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Location
{
    [Table("ServiceAreas")]
    public class ServiceArea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string AreaName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Region { get; set; }

        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal Radius { get; set; } = 10.00m;

        [Required]
        public bool IsActive { get; set; } = true;

        public string Description { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
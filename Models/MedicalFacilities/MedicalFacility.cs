// Models/MedicalFacilities/MedicalFacility.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.MedicalFacilities
{
    [Table("MedicalFacilities")]
    public class MedicalFacility
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11, 8)")]
        public decimal? Longitude { get; set; }

        [StringLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public string Services { get; set; } = string.Empty;

        [StringLength(255)]
        public string OperatingHours { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool Verified { get; set; } = false;

        [Column(TypeName = "decimal(3, 2)")]
        [Range(0.00, 5.00)]
        public decimal AverageRating { get; set; } = 0.00m;

        public int TotalReviews { get; set; } = 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<FacilityReview> Reviews { get; set; } = new List<FacilityReview>();
    }
}
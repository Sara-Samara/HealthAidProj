// Models/Service.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public int? ProviderId { get; set; }

        [StringLength(20)]
        public string? ProviderType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Computed property
        [NotMapped]
        public bool IsFree => !Price.HasValue || Price == 0;
    }
}
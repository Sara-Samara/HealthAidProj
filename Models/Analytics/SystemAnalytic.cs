// Models/Analytics/SystemAnalytic.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Analytics
{
    [Table("SystemAnalytics")]
    public class SystemAnalytic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime AnalyticsDate { get; set; }

        public int TotalUsers { get; set; } = 0;

        public int TotalPatients { get; set; } = 0;

        public int TotalDoctors { get; set; } = 0;

        public int TotalConsultations { get; set; } = 0;

        public int TotalDonations { get; set; } = 0;

        [Column(TypeName = "decimal(15, 2)")]
        public decimal TotalDonationAmount { get; set; } = 0.00m;

        public int TotalEmergencyCases { get; set; } = 0;

        [StringLength(100)]
        public string MostRequestedService { get; set; } = string.Empty;

        [StringLength(100)]
        public string TopRegion { get; set; } = string.Empty;

        [Column(TypeName = "decimal(8, 2)")]
        public decimal? AverageResponseTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
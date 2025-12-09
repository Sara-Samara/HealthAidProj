// Models/Sync/OfflineQueue.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Sync
{
    [Table("OfflineQueues")]
    public class OfflineQueue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActionType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Data { get; set; } = string.Empty;

        [Required]
        public bool IsSynced { get; set; } = false;

        public int SyncAttempts { get; set; } = 0;

        public DateTime? LastSyncAttempt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
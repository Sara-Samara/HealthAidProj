using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Sync
{
    public class SyncRequestDto
    {
        [Required]
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete

        [Required]
        public string EntityType { get; set; } = string.Empty; // Appointment, Message, etc.

        public int? EntityId { get; set; } // Null for Create actions

        [Required]
        public string Data { get; set; } = string.Empty; // JSON Data

        public DateTime Timestamp { get; set; }
    }
}
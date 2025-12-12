using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Messages
{
    public class ChatRequestDto
    {
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int SenderId { get; set; }
        
        [Required(ErrorMessage = "Sender name is required")]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }
}
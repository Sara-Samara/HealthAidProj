using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Messages
{
    public class CreateMessageDto
    {
        [Required(ErrorMessage = "Message content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters")]
        public string Content { get; set; } = string.Empty;

        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "Receiver ID is required")]
        public int ReceiverId { get; set; }
    }
}

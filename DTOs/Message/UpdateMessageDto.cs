using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Messages
{
    public class UpdateMessageDto
    {
        [Required(ErrorMessage = "New content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters")]
        public string NewContent { get; set; } = string.Empty;
    }
}
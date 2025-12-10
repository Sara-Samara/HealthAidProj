using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Messages
{
    public class CreateMessageByNameDto
    {
        [Required(ErrorMessage = "Message content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sender name is required")]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }
}
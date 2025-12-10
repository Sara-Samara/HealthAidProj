using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Messages
{
    public class MarkAsReadDto
    {
        [Required(ErrorMessage = "Sender name is required")]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }
}
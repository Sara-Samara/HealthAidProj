// DTOs/MessageDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsRead { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
    }

    public class CreateMessageDto
    {
        [Required(ErrorMessage = "Message content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sender ID is required")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "Receiver ID is required")]
        public int ReceiverId { get; set; }
    }

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

    public class UpdateMessageDto
    {
        [Required(ErrorMessage = "New content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message content must be between 1 and 1000 characters")]
        public string NewContent { get; set; } = string.Empty;
    }

    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Sender name is required")]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }

    public class MarkAsReadDto
    {
        [Required(ErrorMessage = "Sender name is required")]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }
}
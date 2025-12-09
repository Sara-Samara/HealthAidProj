using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Message
    {
        [Key]

        public int MessageId { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Shortest Message cannnot send Message with less than 10 chachter")]
        [MaxLength(1000, ErrorMessage = "Too leongth Message Can not exeted 1000 charcter")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data Time is required")]
        [DataType(DataType.Date)]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        [DataType(DataType.Date)]
        public DateTime? EditedAt { get; set; }


        public bool IsRead { get; set; } = false;



        [ForeignKey(nameof(Sender))]
        public int SenderId { get; set; }
        public virtual User? Sender { get; set; }

        [ForeignKey(nameof(Receiver))]
        public int ReceiverId { get; set; }
        public virtual User? Receiver { get; set; }

    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Notification
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int NotificationId { get; set; }

        [Required(ErrorMessage = "Notification title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        [Display(Name = "Notification Title")]
        public string Title { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Notification message is required.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 500 characters.")]
        [Display(Name = "Notification Message")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification type is required.")] 
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Type must be between 3 and 50 characters.")]

        [RegularExpression("^(Info|Warning|Alert|Success|Promotional)$",
        ErrorMessage = "Type must be Info, Warning, Alert, Success, or Promotional.")]
        [Display(Name = "Notification Type")]
        public string Type { get; set; } = "Info";

        [Display(Name = "Is Read")]
        public bool IsRead { get; set; } = false;

        [Required(ErrorMessage = "Creation date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [Display(Name = "Creation Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Sender))]
        [Display(Name = "Sender User ID")]
        public int? SenderId { get; set; }

        public virtual User? Sender { get; set; }

        [Required(ErrorMessage = "Receiver ID is required.")] 
        [ForeignKey(nameof(Receiver))]
        [Display(Name = "Receiver User ID")]
        public int ReceiverId { get; set; } 

        public virtual User Receiver { get; set; } = null!; 
    }
}
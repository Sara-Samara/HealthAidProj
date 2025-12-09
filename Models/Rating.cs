using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class Rating
    {
        public int RatingId { get; set; }

        [Required(ErrorMessage = "Target type is required.")]
        [MinLength(3, ErrorMessage = "Target type cannot be less than 3 characters")]
        [MaxLength(50, ErrorMessage = "Target type cannot exceed 50 characters")]
        public string TargetType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Target ID must be a positive number.")]
        public int TargetId { get; set; }

        [Required(ErrorMessage = "Rating value is required.")]
        [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5.")]
        public int Value { get; set; }

        [StringLength(200, ErrorMessage = "Comment cannot exceed 200 characters.")]
        public string Comment { get; set; } = string.Empty; 

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User? User { get; set; } 
    }
}
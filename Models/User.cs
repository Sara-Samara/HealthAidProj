// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MinLength(3, ErrorMessage = "First name must be at least 3 characters long.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(3, ErrorMessage = "Last name must be at least 3 characters long.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password hash is required.")]
        public required string PasswordHash { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(15)]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Country name is required.")]
        [MinLength(3, ErrorMessage = "Country name must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Country name cannot exceed 50 characters.")]
        public required string Country { get; set; }

        [Required(ErrorMessage = "City name is required.")]
        [MinLength(3, ErrorMessage = "City name must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "City name cannot exceed 50 characters.")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Street name is required.")]
        [MinLength(3, ErrorMessage = "Street name must be at least 3 characters.")]
        [MaxLength(100, ErrorMessage = "Street name cannot exceed 100 characters.")]
        public required string Street { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "Patient";

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public bool EmailVerified { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }

        public virtual Doctor? Doctor { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual Donor? Donor { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<HealthGuide> HealthGuides { get; set; } = new List<HealthGuide>();
        public virtual ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
        public virtual ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<PublicAlert> PublicAlerts { get; set; } = new List<PublicAlert>();
    }
}
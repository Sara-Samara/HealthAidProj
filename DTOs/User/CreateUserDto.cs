using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        public string Role { get; set; } = "Patient";
    }
}
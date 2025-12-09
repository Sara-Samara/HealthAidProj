// DTOs/UserDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }

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

    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }

    public class UserFilterDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
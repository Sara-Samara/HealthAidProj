using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Donors
{
    public class CreateDonorDto
    {
        [Required(ErrorMessage = "Organization or Donor Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Organization { get; set; } = string.Empty;

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
    }
}
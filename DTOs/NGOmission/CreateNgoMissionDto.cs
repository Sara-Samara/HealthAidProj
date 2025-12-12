using System.ComponentModel.DataAnnotations;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.DTOs.NgoMissions
{
    public class CreateNgoMissionDto
    {
        [Required(ErrorMessage = "Mission description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DateAfter("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 100 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "NGO ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "NGO ID must be a positive number")]
        public int NGOId { get; set; }
    }
}
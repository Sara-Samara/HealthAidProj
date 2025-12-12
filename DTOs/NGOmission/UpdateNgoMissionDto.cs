using System.ComponentModel.DataAnnotations;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.DTOs.NgoMissions
{
    public class UpdateNgoMissionDto
    {
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string? Description { get; set; }

        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime? StartDate { get; set; }

        [DateAfter("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime? EndDate { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 100 characters")]
        public string? Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "NGO ID must be a positive number")]
        public int? NGOId { get; set; }
    }
}
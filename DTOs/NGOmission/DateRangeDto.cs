using System.ComponentModel.DataAnnotations;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.DTOs.NgoMissions
{
    public class DateRangeDto
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DateAfter("Start", ErrorMessage = "End date must be after start date")]
        public DateTime End { get; set; }
    }
}
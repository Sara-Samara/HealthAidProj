using System.ComponentModel.DataAnnotations;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.DTOs.Equipments
{
    public class MaintenanceScheduleDto
    {
        [Required(ErrorMessage = "Next maintenance date is required")]
        [FutureDate(ErrorMessage = "Next maintenance date must be in the future")]
        public DateTime NextMaintenanceDate { get; set; }

        [StringLength(500, ErrorMessage = "Maintenance notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
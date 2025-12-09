// DTOs/NgoMissionDto.cs
using HealthAidAPI.Helpers;
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.NGOmission
{
    public class NgoMissionDto
    {
        public int NgoMissionId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int NGOId { get; set; }
        public string NGOName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int DaysRemaining { get; set; }
    }

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

    public class NgoMissionFilterDto
    {
        public string? Search { get; set; }
        public string? Location { get; set; }
        public int? NGOId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public string? Status { get; set; } // Upcoming, Ongoing, Completed
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class MissionStatsDto
    {
        public int TotalMissions { get; set; }
        public int UpcomingMissions { get; set; }
        public int OngoingMissions { get; set; }
        public int CompletedMissions { get; set; }
        public Dictionary<string, int> MissionsByLocation { get; set; } = new();
        public Dictionary<int, int> MissionsByNGO { get; set; } = new();
    }

    public class DateRangeDto
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DateAfter("Start", ErrorMessage = "End date must be after start date")]
        public DateTime End { get; set; }
    }
}
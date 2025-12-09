// DTOs/NgoDto.cs
using HealthAidAPI.DTOs.NGOmission;
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.NGO
{
    public class NgoDto
    {
        public int NGOId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string AreaOfWork { get; set; } = string.Empty;
        public string VerifiedStatus { get; set; } = string.Empty;
        public string ContactedPerson { get; set; } = string.Empty;
        public int MissionCount { get; set; }
        public int EquipmentCount { get; set; }
        public int PatientCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateNgoDto
    {
        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Organization name must be between 3 and 100 characters")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area of work is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Area of work must be between 3 and 100 characters")]
        public string AreaOfWork { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verified status is required")]
        [StringLength(20, ErrorMessage = "Verified status cannot exceed 20 characters")]
        public string VerifiedStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Contact person must be between 3 and 50 characters")]
        public string ContactedPerson { get; set; } = string.Empty;
    }

    public class UpdateNgoDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Organization name must be between 3 and 100 characters")]
        public string? OrganizationName { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Area of work must be between 3 and 100 characters")]
        public string? AreaOfWork { get; set; }

        [StringLength(20, ErrorMessage = "Verified status cannot exceed 20 characters")]
        public string? VerifiedStatus { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Contact person must be between 3 and 50 characters")]
        public string? ContactedPerson { get; set; }
    }

    public class NgoFilterDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? AreaOfWork { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class NgoStatsDto
    {
        public int TotalNgos { get; set; }
        public int VerifiedNgos { get; set; }
        public int PendingNgos { get; set; }
        public int RejectedNgos { get; set; }
        public Dictionary<string, int> NgosByArea { get; set; } = new();
        public int TotalMissions { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalPatients { get; set; }
    }

    public class NgoDetailDto
    {
        public int NGOId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string AreaOfWork { get; set; } = string.Empty;
        public string VerifiedStatus { get; set; } = string.Empty;
        public string ContactedPerson { get; set; } = string.Empty;
        public List<NgoMissionDto> Missions { get; set; } = new();
       // public List<EquipmentDto> Equipments { get; set; } = new();
       // public List<PatientDto> Patients { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
    }
}
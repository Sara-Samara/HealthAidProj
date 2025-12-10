namespace HealthAidAPI.DTOs.Patients
{
    public class PatientFilterDto
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public string? BloodType { get; set; }
        public int? NGOId { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool? HasMedicalHistory { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
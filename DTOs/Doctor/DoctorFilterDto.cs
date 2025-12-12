namespace HealthAidAPI.DTOs.Doctors
{
    public class DoctorFilterDto
    {
        public string? Specialization { get; set; }
        public string? Search { get; set; }
        public int? MinYearsExperience { get; set; }
        public int? MaxYearsExperience { get; set; }
        public bool? IsAvailable { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
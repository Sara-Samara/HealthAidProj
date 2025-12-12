namespace HealthAidAPI.DTOs.Consultations
{
    public class ConsultationFilterDto
    {
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? Status { get; set; }
        public string? Mode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
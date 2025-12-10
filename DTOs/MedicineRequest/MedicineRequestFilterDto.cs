namespace HealthAidAPI.DTOs.MedicineRequests
{
    public class MedicineRequestFilterDto
    {
        public string? Search { get; set; }
        public string? MedicineName { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Urgency { get; set; }
        public int? PatientId { get; set; }
        public bool? IsUrgent { get; set; }
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public DateTime? RequiredByDateFrom { get; set; }
        public DateTime? RequiredByDateTo { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}
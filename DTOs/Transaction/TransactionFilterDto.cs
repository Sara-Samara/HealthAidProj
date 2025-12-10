namespace HealthAidAPI.DTOs.Transactions
{
    public class TransactionFilterDto
    {
        public int? UserId { get; set; }
        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public string? Status { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
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
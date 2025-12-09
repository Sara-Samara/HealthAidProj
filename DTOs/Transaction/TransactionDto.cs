using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public int? UserId { get; set; }
        public ConsultationDto? Consultation { get; set; }
        public DonationDto? Donation { get; set; }
        public MedicineRequestDto? MedicineRequest { get; set; }
        public UserDto? User { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string FormattedAmount => Amount.ToString("C");
    }

    public class CreateTransactionDto
    {
        [Required]
        [Range(0.01, 1000000.00)]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        public DateTime? Date { get; set; }

        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public int? UserId { get; set; }
    }

    public class UpdateTransactionDto
    {
        [Range(0.01, 1000000.00)]
        public decimal? Amount { get; set; }

        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string? Status { get; set; }

        public DateTime? Date { get; set; }

        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public int? UserId { get; set; }
    }

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
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class TransactionStatsDto
    {
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, decimal> RevenueByType { get; set; } = new();
        public int TransactionsThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPending { get; set; }
        public decimal TotalCompleted { get; set; }
        public decimal TotalFailed { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal QuarterlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal AverageTransactionValue { get; set; }
    }

    public class UpdateTransactionStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Completed|Failed|Refunded|Cancelled)$")]
        public string Status { get; set; } = string.Empty;
    }

    public class DonationDto
    {
        public int DonationId { get; set; }
        public decimal Amount { get; set; }
        public string? DonorName { get; set; }
        // Add other donation properties as needed
    }

}
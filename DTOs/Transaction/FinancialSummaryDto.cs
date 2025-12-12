namespace HealthAidAPI.DTOs.Transactions
{
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
}
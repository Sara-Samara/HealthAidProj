using System.Collections.Generic;

namespace HealthAidAPI.DTOs.Transactions
{
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
}
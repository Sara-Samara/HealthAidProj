using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter);
        Task<TransactionDto?> GetTransactionByIdAsync(int id);
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto transactionDto);
        Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto transactionDto);
        Task<bool> DeleteTransactionAsync(int id);
        Task<bool> UpdateTransactionStatusAsync(int id, string status);
        Task<IEnumerable<TransactionDto>> GetTransactionsByUserAsync(int userId);
        Task<IEnumerable<TransactionDto>> GetTransactionsByConsultationAsync(int consultationId);
        Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<TransactionStatsDto> GetTransactionStatsAsync();
        Task<FinancialSummaryDto> GetFinancialSummaryAsync();
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync();
    }
}
using HealthAidAPI.DTOs.Transactions;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter);

        Task<TransactionDto?> GetTransactionByIdAsync(int id);

        Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto);

        Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto dto, int userId);

        Task<bool> DeleteTransactionAsync(int id, int userId);

        Task<bool> UpdateTransactionStatusAsync(int id, string status);

        Task<IEnumerable<TransactionDto>> GetTransactionsByUserAsync(int userId);

        Task<IEnumerable<TransactionDto>> GetTransactionsByConsultationAsync(int consultationId);

        Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end);

        Task<TransactionStatsDto> GetTransactionStatsAsync();

        Task<FinancialSummaryDto> GetFinancialSummaryAsync();

        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync();
    }
}

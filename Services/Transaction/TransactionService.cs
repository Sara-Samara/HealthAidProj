using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.Helpers;
using HealthAidAPI.DTOs.Transactions;
using HealthAidAPI.DTOs.Donations;
using HealthAidAPI.DTOs.Consultations;
using HealthAidAPI.DTOs.MedicineRequests;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ApplicationDbContext context, IMapper mapper, ILogger<TransactionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // ============================================================
        // GET ALL (Admin Only)
        // ============================================================
        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter)
        {
            var query = _context.Transactions
                .Include(t => t.Consultation)
                .Include(t => t.Donation)
                .Include(t => t.MedicineRequest)
                .Include(t => t.User)
                .AsQueryable();

            // ============================
            // Filtering
            // ============================
            if (filter.UserId.HasValue)
                query = query.Where(t => t.UserId == filter.UserId.Value);

            if (filter.ConsultationId.HasValue)
                query = query.Where(t => t.ConsultationId == filter.ConsultationId.Value);

            if (filter.DonationId.HasValue)
                query = query.Where(t => t.DonationId == filter.DonationId.Value);

            if (filter.MedicineRequestId.HasValue)
                query = query.Where(t => t.MedicineRequestId == filter.MedicineRequestId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.Date <= filter.EndDate.Value);

            // ============================
            // Sorting
            // ============================
            query = filter.SortBy?.ToLower() switch
            {
                "amount" => filter.SortDesc
                    ? query.OrderByDescending(t => t.Amount)
                    : query.OrderBy(t => t.Amount),

                "date" => filter.SortDesc
                    ? query.OrderByDescending(t => t.Date)
                    : query.OrderBy(t => t.Date),

                _ => query.OrderByDescending(t => t.TransactionId)
            };

            // ============================
            // Pagination base count
            // ============================
            int totalCount = await query.CountAsync();

            // ============================
            // Fetch + Map (IMPORTANT FIX)
            // ============================
            var entities = await query.ToListAsync();
            var data = _mapper.Map<List<TransactionDto>>(entities);

            return new PagedResult<TransactionDto>(data, totalCount);
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Consultation)
                .Include(t => t.Donation)
                .Include(t => t.MedicineRequest)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto)
        {
            if (!dto.UserId.HasValue || !await _context.Users.AnyAsync(u => u.Id == dto.UserId.Value))
                throw new ArgumentException("Invalid user for transaction");

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Status = dto.Status,
                Date = dto.Date ?? DateTime.UtcNow,
                ConsultationId = dto.ConsultationId,
                DonationId = dto.DonationId,
                MedicineRequestId = dto.MedicineRequestId,
                UserId = dto.UserId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return await GetTransactionByIdAsync(transaction.TransactionId);
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto dto, int userId)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
                return null;

            bool isAdmin = await _context.Users.AnyAsync(u => u.Id == userId && u.Role == "Admin");

            if (!isAdmin && transaction.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to update this transaction.");

            if (dto.Amount.HasValue)
                transaction.Amount = dto.Amount.Value;

            if (!string.IsNullOrEmpty(dto.Status))
                transaction.Status = dto.Status;

            if (dto.Date.HasValue)
                transaction.Date = dto.Date.Value;

            await _context.SaveChangesAsync();

            return await GetTransactionByIdAsync(id);
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task<bool> DeleteTransactionAsync(int id, int userId)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
                return false;

            bool isAdmin = await _context.Users.AnyAsync(u => u.Id == userId && u.Role == "Admin");

            if (!isAdmin && transaction.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to delete this transaction.");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // UPDATE STATUS (Admin Only)
        // ============================================================
        public async Task<bool> UpdateTransactionStatusAsync(int id, string status)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
                return false;

            transaction.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // GET TRANSACTIONS BY USER
        // ============================================================
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserAsync(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .Select(t => _mapper.Map<TransactionDto>(t))
                .ToListAsync();
        }

        // ============================================================
        // GET BY CONSULTATION
        // ============================================================
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByConsultationAsync(int consultationId)
        {
            return await _context.Transactions
                .Where(t => t.ConsultationId == consultationId)
                .OrderByDescending(t => t.Date)
                .Select(t => _mapper.Map<TransactionDto>(t))
                .ToListAsync();
        }

        // ============================================================
        // GET BY DATE RANGE
        // ============================================================
        public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Transactions
                .Where(t => t.Date >= start && t.Date <= end)
                .OrderByDescending(t => t.Date)
                .Select(t => _mapper.Map<TransactionDto>(t))
                .ToListAsync();
        }

        // ============================================================
        // STATS
        // ============================================================
        public async Task<TransactionStatsDto> GetTransactionStatsAsync()
        {
            return new TransactionStatsDto
            {
                TotalTransactions = await _context.Transactions.CountAsync(),
                CompletedTransactions = await _context.Transactions.CountAsync(t => t.Status == "Completed"),
                PendingTransactions = await _context.Transactions.CountAsync(t => t.Status == "Pending"),
                FailedTransactions = await _context.Transactions.CountAsync(t => t.Status == "Failed")
            };
        }

        // ============================================================
        // FINANCIAL SUMMARY
        // ============================================================
        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync()
        {
            var completed = await _context.Transactions
                .Where(t => t.Status == "Completed")
                .ToListAsync();

            return new FinancialSummaryDto
            {
                TotalRevenue = completed.Sum(t => t.Amount),
                TransactionCount = completed.Count,
                AverageTransactionValue = completed.Count == 0 ? 0 : completed.Average(t => t.Amount)
            };
        }

        // ============================================================
        // TOTAL REVENUE
        // ============================================================
        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Transactions.Where(t => t.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            return await query.SumAsync(t => t.Amount);
        }

        // ============================================================
        // PENDING TRANSACTIONS
        // ============================================================
        public async Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync()
        {
            return await _context.Transactions
                .Where(t => t.Status == "Pending")
                .OrderByDescending(t => t.Date)
                .Select(t => _mapper.Map<TransactionDto>(t))
                .ToListAsync();
        }
    }
}

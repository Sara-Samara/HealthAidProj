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

        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter)
        {
            try
            {
                var query = _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .AsQueryable();

                // Apply filters
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

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "amount" => filter.SortDesc ?
                        query.OrderByDescending(t => t.Amount) :
                        query.OrderBy(t => t.Amount),
                    "date" => filter.SortDesc ?
                        query.OrderByDescending(t => t.Date) :
                        query.OrderBy(t => t.Date),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(t => t.Status) :
                        query.OrderBy(t => t.Status),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(t => t.TransactionId) :
                        query.OrderBy(t => t.TransactionId)
                };

                var totalCount = await query.CountAsync();

                var transactions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        Amount = t.Amount,
                        Status = t.Status,
                        Date = t.Date,
                        ConsultationId = t.ConsultationId,
                        DonationId = t.DonationId,
                        MedicineRequestId = t.MedicineRequestId,
                        UserId = t.UserId,
                        Consultation = t.Consultation != null ? new ConsultationDto
                        {
                            ConsultationId = t.Consultation.ConsultationId,
                            ConsDate = t.Consultation.ConsDate,
                            Status = t.Consultation.Status
                        } : null,
                        Donation = t.Donation != null ? new DonationDto
                        {
                            DonationId = t.Donation.DonationId,
                            Amount = t.Donation.Amount,
                            DonorName = t.Donation.DonorName
                        } : null,
                        MedicineRequest = t.MedicineRequest != null ? new MedicineRequestDto
                        {
                            MedicineRequestId = t.MedicineRequest.MedicineRequestId,
                            MedicineName = t.MedicineRequest.MedicineName,
                            Status = t.MedicineRequest.Status
                        } : null,
                        User = t.User != null ? new UserDto
                        {
                            Id = t.User.Id,
                            FirstName = t.User.FirstName,
                            LastName = t.User.LastName,
                            Email = t.User.Email
                        } : null,
                        TransactionType = GetTransactionType(t)
                    })
                    .ToListAsync();

                return new PagedResult<TransactionDto>(transactions, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions with filter {@Filter}", filter);
                throw;
            }
        }

        public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TransactionId == id);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found", id);
                    return null;
                }

                var transactionDto = _mapper.Map<TransactionDto>(transaction);
                transactionDto.TransactionType = GetTransactionType(transaction);

                return transactionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction by ID: {TransactionId}", id);
                throw;
            }
        }

        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto transactionDto)
        {
            try
            {
                // Validate related entities if provided
                if (transactionDto.ConsultationId.HasValue)
                {
                    var consultationExists = await _context.Consultations
                        .AnyAsync(c => c.ConsultationId == transactionDto.ConsultationId.Value);
                    if (!consultationExists)
                        throw new ArgumentException("Consultation not found");
                }

                if (transactionDto.DonationId.HasValue)
                {
                    var donationExists = await _context.Donations
                        .AnyAsync(d => d.DonationId == transactionDto.DonationId.Value);
                    if (!donationExists)
                        throw new ArgumentException("Donation not found");
                }

                if (transactionDto.MedicineRequestId.HasValue)
                {
                    var medicineRequestExists = await _context.MedicineRequests
                        .AnyAsync(m => m.MedicineRequestId == transactionDto.MedicineRequestId.Value);
                    if (!medicineRequestExists)
                        throw new ArgumentException("Medicine request not found");
                }

                if (transactionDto.UserId.HasValue)
                {
                    var userExists = await _context.Users
                        .AnyAsync(u => u.Id == transactionDto.UserId.Value);
                    if (!userExists)
                        throw new ArgumentException("User not found");
                }

                var transaction = new Transaction
                {
                    Amount = transactionDto.Amount,
                    Status = transactionDto.Status,
                    Date = transactionDto.Date ?? DateTime.UtcNow,
                    ConsultationId = transactionDto.ConsultationId,
                    DonationId = transactionDto.DonationId,
                    MedicineRequestId = transactionDto.MedicineRequestId,
                    UserId = transactionDto.UserId
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} created successfully with amount {Amount}",
                    transaction.TransactionId, transactionDto.Amount);

                return await GetTransactionByIdAsync(transaction.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction with data {@TransactionDto}", transactionDto);
                throw;
            }
        }

        public async Task<TransactionDto?> UpdateTransactionAsync(int id, UpdateTransactionDto transactionDto)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TransactionId == id);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for update", id);
                    return null;
                }

                // Update only provided fields
                if (transactionDto.Amount.HasValue)
                    transaction.Amount = transactionDto.Amount.Value;

                if (!string.IsNullOrEmpty(transactionDto.Status))
                    transaction.Status = transactionDto.Status;

                if (transactionDto.Date.HasValue)
                    transaction.Date = transactionDto.Date.Value;

                if (transactionDto.ConsultationId.HasValue)
                {
                    var consultationExists = await _context.Consultations
                        .AnyAsync(c => c.ConsultationId == transactionDto.ConsultationId.Value);
                    if (!consultationExists)
                        throw new ArgumentException("Consultation not found");
                    transaction.ConsultationId = transactionDto.ConsultationId.Value;
                }

                if (transactionDto.DonationId.HasValue)
                {
                    var donationExists = await _context.Donations
                        .AnyAsync(d => d.DonationId == transactionDto.DonationId.Value);
                    if (!donationExists)
                        throw new ArgumentException("Donation not found");
                    transaction.DonationId = transactionDto.DonationId.Value;
                }

                if (transactionDto.MedicineRequestId.HasValue)
                {
                    var medicineRequestExists = await _context.MedicineRequests
                        .AnyAsync(m => m.MedicineRequestId == transactionDto.MedicineRequestId.Value);
                    if (!medicineRequestExists)
                        throw new ArgumentException("Medicine request not found");
                    transaction.MedicineRequestId = transactionDto.MedicineRequestId.Value;
                }

                if (transactionDto.UserId.HasValue)
                {
                    var userExists = await _context.Users
                        .AnyAsync(u => u.Id == transactionDto.UserId.Value);
                    if (!userExists)
                        throw new ArgumentException("User not found");
                    transaction.UserId = transactionDto.UserId.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} updated successfully", id);
                return await GetTransactionByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction with ID: {TransactionId} and data: {@TransactionDto}", id, transactionDto);
                throw;
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for deletion", id);
                    return false;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction with ID: {TransactionId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateTransactionStatusAsync(int id, string status)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for status update", id);
                    return false;
                }

                transaction.Status = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transaction {TransactionId} status updated to {Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction status for ID: {TransactionId} to {Status}", id, status);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserAsync(int userId)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByConsultationAsync(int consultationId)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.User)
                    .Where(t => t.ConsultationId == consultationId)
                    .OrderByDescending(t => t.Date)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for consultation ID: {ConsultationId}", consultationId);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .OrderBy(t => t.Date)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<TransactionStatsDto> GetTransactionStatsAsync()
        {
            try
            {
                var totalTransactions = await _context.Transactions.CountAsync();

                var statusDistribution = await _context.Transactions
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                var completedTransactions = await _context.Transactions
                    .CountAsync(t => t.Status == "Completed");

                var pendingTransactions = await _context.Transactions
                    .CountAsync(t => t.Status == "Pending");

                var failedTransactions = await _context.Transactions
                    .CountAsync(t => t.Status == "Failed");

                var totalRevenue = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .SumAsync(t => t.Amount);

                var averageTransactionAmount = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .AverageAsync(t => t.Amount);

                var revenueByType = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .GroupBy(t => GetTransactionType(t))
                    .Select(g => new { Type = g.Key, Revenue = g.Sum(t => t.Amount) })
                    .ToDictionaryAsync(x => x.Type, x => x.Revenue);

                var transactionsThisMonth = await _context.Transactions
                    .CountAsync(t => t.Date >= DateTime.UtcNow.AddMonths(-1));

                var revenueThisMonth = await _context.Transactions
                    .Where(t => t.Status == "Completed" && t.Date >= DateTime.UtcNow.AddMonths(-1))
                    .SumAsync(t => t.Amount);

                return new TransactionStatsDto
                {
                    TotalTransactions = totalTransactions,
                    CompletedTransactions = completedTransactions,
                    PendingTransactions = pendingTransactions,
                    FailedTransactions = failedTransactions,
                    TotalRevenue = totalRevenue,
                    AverageTransactionAmount = Math.Round(averageTransactionAmount, 2),
                    StatusDistribution = statusDistribution,
                    RevenueByType = revenueByType,
                    TransactionsThisMonth = transactionsThisMonth,
                    RevenueThisMonth = revenueThisMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction statistics");
                throw;
            }
        }

        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfQuarter = new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1);
                var startOfYear = new DateTime(now.Year, 1, 1);

                var totalRevenue = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .SumAsync(t => t.Amount);

                var totalPending = await _context.Transactions
                    .Where(t => t.Status == "Pending")
                    .SumAsync(t => t.Amount);

                var totalCompleted = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .SumAsync(t => t.Amount);

                var totalFailed = await _context.Transactions
                    .Where(t => t.Status == "Failed")
                    .SumAsync(t => t.Amount);

                var monthlyRevenue = await _context.Transactions
                    .Where(t => t.Status == "Completed" && t.Date >= startOfMonth)
                    .SumAsync(t => t.Amount);

                var quarterlyRevenue = await _context.Transactions
                    .Where(t => t.Status == "Completed" && t.Date >= startOfQuarter)
                    .SumAsync(t => t.Amount);

                var yearlyRevenue = await _context.Transactions
                    .Where(t => t.Status == "Completed" && t.Date >= startOfYear)
                    .SumAsync(t => t.Amount);

                var transactionCount = await _context.Transactions
                    .Where(t => t.Status == "Completed")
                    .CountAsync();

                var averageTransactionValue = transactionCount > 0 ? totalCompleted / transactionCount : 0;

                return new FinancialSummaryDto
                {
                    TotalRevenue = totalRevenue,
                    TotalPending = totalPending,
                    TotalCompleted = totalCompleted,
                    TotalFailed = totalFailed,
                    MonthlyRevenue = monthlyRevenue,
                    QuarterlyRevenue = quarterlyRevenue,
                    YearlyRevenue = yearlyRevenue,
                    TransactionCount = transactionCount,
                    AverageTransactionValue = Math.Round(averageTransactionValue, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial summary");
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Transactions.Where(t => t.Status == "Completed");

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.Date <= endDate.Value);

                var totalRevenue = await query.SumAsync(t => t.Amount);
                return totalRevenue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue");
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync()
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Consultation)
                    .Include(t => t.Donation)
                    .Include(t => t.MedicineRequest)
                    .Include(t => t.User)
                    .Where(t => t.Status == "Pending")
                    .OrderBy(t => t.Date)
                    .Select(t => _mapper.Map<TransactionDto>(t))
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transactions");
                throw;
            }
        }

        // Helper method to determine transaction type
        private string GetTransactionType(Transaction transaction)
        {
            if (transaction.ConsultationId.HasValue) return "Consultation";
            if (transaction.DonationId.HasValue) return "Donation";
            if (transaction.MedicineRequestId.HasValue) return "Medicine";
            return "Other";
        }
    }
}
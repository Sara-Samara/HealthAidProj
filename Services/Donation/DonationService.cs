using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Donations;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;

        public DonationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DonationDto>> GetDonationsAsync(DonationFilterDto filter)
        {
            var query = _context.Donations
                .Include(d => d.Donor).ThenInclude(dor => dor!.User)
                .Include(d => d.Sponsorship)
                .AsQueryable();

            // Filtering
            if (filter.SponsorshipId.HasValue)
                query = query.Where(d => d.SponsorshipId == filter.SponsorshipId);

            if (filter.DonorId.HasValue)
                query = query.Where(d => d.DonorId == filter.DonorId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(d => d.Status == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(d => d.DonationDate >= filter.FromDate);

            if (filter.ToDate.HasValue)
                query = query.Where(d => d.DonationDate <= filter.ToDate);

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "amount" => filter.SortDesc ? query.OrderByDescending(d => d.Amount) : query.OrderBy(d => d.Amount),
                "date" => filter.SortDesc ? query.OrderByDescending(d => d.DonationDate) : query.OrderBy(d => d.DonationDate),
                _ => filter.SortDesc ? query.OrderByDescending(d => d.DonationId) : query.OrderBy(d => d.DonationId)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(d => MapToDto(d))
                .ToListAsync();

            return new PagedResult<DonationDto>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<DonationDto?> GetDonationByIdAsync(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor).ThenInclude(dor => dor!.User)
                .Include(d => d.Sponsorship)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            return donation == null ? null : MapToDto(donation);
        }

        public async Task<DonationDto> CreateDonationAsync(CreateDonationDto dto)
        {
            // التحقق من وجود الكفالة
            var sponsorship = await _context.Sponsorships.FindAsync(dto.SponsorshipId);
            if (sponsorship == null)
                throw new ArgumentException("Sponsorship not found");

            var donation = new Donation
            {
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                DonationType = dto.DonationType,
                SponsorshipId = dto.SponsorshipId,
                DonorId = dto.DonorId,
                IsAnonymous = dto.IsAnonymous,
                ReceiveUpdates = dto.ReceiveUpdates,
                Notes = dto.Notes,
                Message = dto.Message,
                Status = "Pending",
                DonationDate = DateTime.UtcNow,
                DateDonated = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // ملاحظة: تحديث مبلغ الكفالة يتم عادة عندما تصبح الحالة "Completed"
            // سيتم التعامل مع ذلك في دالة التحديث

            // إعادة تحميل البيانات للعرض
            await _context.Entry(donation).Reference(d => d.Sponsorship).LoadAsync();
            if (donation.DonorId.HasValue)
                await _context.Entry(donation).Reference(d => d.Donor).Query().Include(dor => dor.User).LoadAsync();

            return MapToDto(donation);
        }

        public async Task<DonationDto?> UpdateDonationStatusAsync(int id, UpdateDonationStatusDto dto)
        {
            var donation = await _context.Donations
                .Include(d => d.Sponsorship)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            if (donation == null) return null;

            // التحقق من تغيير الحالة إلى Completed لأول مرة لتحديث الكفالة والمتبرع
            bool justCompleted = donation.Status != "Completed" && dto.Status == "Completed";

            donation.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.TransactionId))
                donation.TransactionId = dto.TransactionId;

            if (justCompleted)
            {
                donation.ProcessedDate = DateTime.UtcNow;

                // تحديث مبلغ الكفالة
                if (donation.Sponsorship != null)
                {
                    donation.Sponsorship.AmountRaised += donation.Amount;
                    // فحص إذا اكتملت الكفالة
                    if (donation.Sponsorship.AmountRaised >= donation.Sponsorship.GoalAmount)
                    {
                        // يمكن تحديث حالة الكفالة هنا إذا لزم الأمر
                    }
                }

                // تحديث إجمالي تبرعات المتبرع
                if (donation.Donor != null)
                {
                    donation.Donor.TotalDonated += donation.Amount;
                }
            }

            await _context.SaveChangesAsync();
            return MapToDto(donation);
        }

        public async Task<DonationStatsDto> GetDonationStatsAsync()
        {
            var stats = new DonationStatsDto
            {
                TotalDonations = await _context.Donations.CountAsync(),
                TotalAmountRaised = await _context.Donations.Where(d => d.Status == "Completed").SumAsync(d => d.Amount),
                CompletedDonationsCount = await _context.Donations.CountAsync(d => d.Status == "Completed"),
                PendingDonationsCount = await _context.Donations.CountAsync(d => d.Status == "Pending"),
                AverageDonationAmount = await _context.Donations.Where(d => d.Status == "Completed").AverageAsync(d => (decimal?)d.Amount) ?? 0,
                DonationsByMethod = await _context.Donations
                    .GroupBy(d => d.PaymentMethod)
                    .Select(g => new { Method = g.Key, Total = g.Sum(d => d.Amount) })
                    .ToDictionaryAsync(x => x.Method, x => x.Total)
            };

            return stats;
        }

        public async Task<List<DonationDto>> GetDonationsByDonorAsync(int donorId)
        {
            var donations = await _context.Donations
                .Where(d => d.DonorId == donorId)
                .Include(d => d.Sponsorship)
                .OrderByDescending(d => d.DonationDate)
                .Select(d => MapToDto(d))
                .ToListAsync();

            return donations;
        }

        public async Task<List<DonationDto>> GetRecentDonationsAsync(int count = 5)
        {
            var donations = await _context.Donations
                .Include(d => d.Donor).ThenInclude(dor => dor!.User)
                .Include(d => d.Sponsorship)
                .OrderByDescending(d => d.DonationDate)
                .Take(count)
                .Select(d => MapToDto(d))
                .ToListAsync();

            return donations;
        }

        private static DonationDto MapToDto(Donation entity)
        {
            return new DonationDto
            {
                DonationId = entity.DonationId,
                Amount = entity.Amount,
                DonorName = entity.DonorName, // خاصية محسوبة في المودل
                DonorEmail = entity.Donor?.User?.Email,
                DonorId = entity.DonorId,
                SponsorshipId = entity.SponsorshipId,
                SponsorshipTitle = entity.Sponsorship?.GoalDescription ?? "Unknown",
                DonationDate = entity.DonationDate,
                PaymentMethod = entity.PaymentMethod,
                Status = entity.Status,
                DonationType = entity.DonationType,
                IsAnonymous = entity.IsAnonymous,
                Message = entity.Message,
                TransactionId = entity.TransactionId
            };
        }
    }
}
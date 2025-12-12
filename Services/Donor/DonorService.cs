using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Donors;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class DonorService : IDonorService
    {
        private readonly ApplicationDbContext _context;

        public DonorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<DonorDto>> GetDonorsAsync(DonorFilterDto filter)
        {
            var query = _context.Donors
                .Include(d => d.User)
                .Include(d => d.Donations)
                .AsQueryable();

            // 1. Filtering
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(d =>
                    d.Organization.ToLower().Contains(search) ||
                    (d.User != null && (d.User.FirstName.ToLower().Contains(search) || d.User.LastName.ToLower().Contains(search)))
                );
            }

            if (filter.MinTotalDonated.HasValue)
                query = query.Where(d => d.TotalDonated >= filter.MinTotalDonated.Value);

            if (filter.MaxTotalDonated.HasValue)
                query = query.Where(d => d.TotalDonated <= filter.MaxTotalDonated.Value);

            // 2. Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "total" => filter.SortDesc ? query.OrderByDescending(d => d.TotalDonated) : query.OrderBy(d => d.TotalDonated),
                "organization" => filter.SortDesc ? query.OrderByDescending(d => d.Organization) : query.OrderBy(d => d.Organization),
                _ => filter.SortDesc ? query.OrderByDescending(d => d.DonorId) : query.OrderBy(d => d.DonorId)
            };

            // 3. Pagination
            var totalCount = await query.CountAsync();
            var donors = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(d => MapToDto(d))
                .ToListAsync();

            
            return new PagedResult<DonorDto>(donors, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<DonorDto?> GetDonorByIdAsync(int id)
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .Include(d => d.Donations)
                .FirstOrDefaultAsync(d => d.DonorId == id);

            return donor == null ? null : MapToDto(donor);
        }

        public async Task<DonorDto?> GetDonorByUserIdAsync(int userId)
        {
            var donor = await _context.Donors
                .Include(d => d.User)
                .Include(d => d.Donations)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            return donor == null ? null : MapToDto(donor);
        }

        public async Task<DonorDto> CreateDonorAsync(CreateDonorDto dto)
        {
            // تحقق من وجود المستخدم
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                throw new ArgumentException("User not found");

            // تحقق إذا كان المستخدم مسجلاً كمتبرع مسبقاً
            var exists = await _context.Donors.AnyAsync(d => d.UserId == dto.UserId);
            if (exists)
                throw new InvalidOperationException("This user is already registered as a donor.");

            var donor = new Donor
            {
                UserId = dto.UserId,
                Organization = dto.Organization,
                TotalDonated = 0 // يبدأ بصفر
            };

            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            // إعادة تحميل البيانات مع المستخدم للإرجاع
            await _context.Entry(donor).Reference(d => d.User).LoadAsync();

            return MapToDto(donor);
        }

        public async Task<DonorDto?> UpdateDonorAsync(int id, UpdateDonorDto dto)
        {
            var donor = await _context.Donors.Include(d => d.User).FirstOrDefaultAsync(d => d.DonorId == id);
            if (donor == null) return null;

            if (!string.IsNullOrEmpty(dto.Organization))
                donor.Organization = dto.Organization;

            await _context.SaveChangesAsync();
            return MapToDto(donor);
        }

        public async Task<bool> DeleteDonorAsync(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null) return false;

            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DonorDto>> GetTopDonorsAsync(int count = 5)
        {
            var donors = await _context.Donors
                .Include(d => d.User)
                .OrderByDescending(d => d.TotalDonated)
                .Take(count)
                .Select(d => MapToDto(d))
                .ToListAsync();

            return donors;
        }

        // Helper Method for Mapping
        private static DonorDto MapToDto(Donor entity)
        {
            return new DonorDto
            {
                DonorId = entity.DonorId,
                Organization = entity.Organization,
                TotalDonated = entity.TotalDonated,
                UserId = entity.UserId,
                DonorName = entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}" : "Unknown",
                Email = entity.User?.Email ?? "",
                DonationCount = entity.Donations?.Count ?? 0,
                JoinedAt = entity.User?.CreatedAt ?? DateTime.MinValue
            };
        }
    }
}
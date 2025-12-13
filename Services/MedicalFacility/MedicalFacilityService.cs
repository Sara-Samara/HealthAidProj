using Microsoft.EntityFrameworkCore;
using HealthAidAPI.Data;
using HealthAidAPI.Models.MedicalFacilities;
using HealthAidAPI.DTOs.MedicalFacilities;
using HealthAidAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HealthAidAPI.Services.Implementations
{
    public class MedicalFacilityService : IMedicalFacilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        public MedicalFacilityService(ApplicationDbContext context , IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<MedicalFacilityDto>> GetMedicalFacilitiesAsync(string? type, bool? verified, decimal? minRating)
        {
            string cacheKey = $"facilities_{type}_{verified}_{minRating}";

            if (!_cache.TryGetValue(cacheKey, out List<MedicalFacilityDto>? cachedFacilities))
            {
                var query = _context.MedicalFacilities.AsQueryable();

                if (!string.IsNullOrEmpty(type)) query = query.Where(f => f.Type == type);
                if (verified.HasValue) query = query.Where(f => f.Verified == verified.Value);
                if (minRating.HasValue) query = query.Where(f => f.AverageRating >= minRating.Value);

                var facilities = await query.ToListAsync();

                cachedFacilities = facilities.Select(MapToDto).ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, cachedFacilities, cacheEntryOptions);
            }

            return cachedFacilities!;
        }

        public async Task<MedicalFacilityDto?> GetMedicalFacilityByIdAsync(int id)
        {
            var facility = await _context.MedicalFacilities
                .Include(f => f.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (facility == null) return null;

            return MapToDto(facility);
        }

        public async Task<MedicalFacilityDto> CreateMedicalFacilityAsync(CreateMedicalFacilityDto dto)
        {
            var facility = new MedicalFacility
            {
                Name = dto.Name,
                Type = dto.Type,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ContactNumber = dto.ContactNumber,
                Email = dto.Email,
                Services = dto.Services,
                OperatingHours = dto.OperatingHours,
                IsActive = true,
                Verified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MedicalFacilities.Add(facility);
            await _context.SaveChangesAsync();

            return MapToDto(facility);
        }

        public async Task<MedicalFacilityDto?> UpdateMedicalFacilityAsync(int id, UpdateMedicalFacilityDto dto)
        {
            var facility = await _context.MedicalFacilities.FindAsync(id);
            if (facility == null) return null;

            // تحديث الحقول فقط إذا لم تكن null
            if (dto.Name != null) facility.Name = dto.Name;
            if (dto.Type != null) facility.Type = dto.Type;
            if (dto.Address != null) facility.Address = dto.Address;
            if (dto.Latitude != null) facility.Latitude = dto.Latitude;
            if (dto.Longitude != null) facility.Longitude = dto.Longitude;
            if (dto.ContactNumber != null) facility.ContactNumber = dto.ContactNumber;
            if (dto.Email != null) facility.Email = dto.Email;
            if (dto.Services != null) facility.Services = dto.Services;
            if (dto.OperatingHours != null) facility.OperatingHours = dto.OperatingHours;

            await _context.SaveChangesAsync();

            return MapToDto(facility);
        }

        public async Task<FacilityReviewDto> AddFacilityReviewAsync(int facilityId, CreateFacilityReviewDto dto)
        {
            var review = new FacilityReview
            {
                FacilityId = facilityId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.FacilityReviews.Add(review);
            await _context.SaveChangesAsync(); // حفظ المراجعة أولاً

            // تحديث التقييم العام للمنشأة
            await UpdateFacilityRatingInternal(facilityId);

            // نحتاج تحميل المستخدم لإرجاع الاسم في الـ DTO
            await _context.Entry(review).Reference(r => r.User).LoadAsync();

            return new FacilityReviewDto
            {
                Id = review.Id,
                FacilityId = review.FacilityId,
                UserId = review.UserId,
                UserName = review.User.FirstName + " " + review.User.LastName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<List<MedicalFacilityDto>> GetNearbyFacilitiesAsync(decimal latitude, decimal longitude, decimal radius, string? type)
        {
            var facilities = await _context.MedicalFacilities
                .Where(f => f.IsActive && f.Verified)
                .ToListAsync();

            // الفلترة في الذاكرة (لأن حساب المسافة المعقد غير مدعوم مباشرة في كل قواعد البيانات بدون إضافات)
            var nearbyFacilities = facilities
                .Where(f => f.Latitude.HasValue && f.Longitude.HasValue)
                .Where(f => CalculateDistance(latitude, longitude, f.Latitude!.Value, f.Longitude!.Value) <= (double)radius)
                .ToList();

            if (!string.IsNullOrEmpty(type))
                nearbyFacilities = nearbyFacilities.Where(f => f.Type == type).ToList();

            return nearbyFacilities.Select(MapToDto).ToList();
        }

        // ==========================================================
        // Helper Methods
        // ==========================================================

        private async Task UpdateFacilityRatingInternal(int facilityId)
        {
            var reviews = await _context.FacilityReviews
                .Where(r => r.FacilityId == facilityId)
                .ToListAsync();

            if (reviews.Any())
            {
                var averageRating = reviews.Average(r => r.Rating);
                var totalReviews = reviews.Count;

                var facility = await _context.MedicalFacilities.FindAsync(facilityId);
                if (facility != null)
                {
                    facility.AverageRating = (decimal)averageRating;
                    facility.TotalReviews = totalReviews;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // دالة بسيطة لحساب المسافة التقريبية (يمكن استبدالها بـ Haversine)
        private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            // تحويل بسيط للمسافة (تقريبي جداً ولا يأخذ انحناء الأرض في الاعتبار بدقة)
            // يفضل استخدام مكتبة مثل NetTopologySuite في المشاريع الحقيقية
            double rLat1 = (double)lat1;
            double rLon1 = (double)lon1;
            double rLat2 = (double)lat2;
            double rLon2 = (double)lon2;

            // Haversine Formula (تقريبي)
            var p = 0.017453292519943295;    // Math.PI / 180
            var a = 0.5 - Math.Cos((rLat2 - rLat1) * p) / 2 +
                    Math.Cos(rLat1 * p) * Math.Cos(rLat2 * p) *
                    (1 - Math.Cos((rLon2 - rLon1) * p)) / 2;

            return 12742 * Math.Asin(Math.Sqrt(a)); // 2 * R; R = 6371 km
        }

        private static MedicalFacilityDto MapToDto(MedicalFacility entity)
        {
            return new MedicalFacilityDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                Address = entity.Address,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                ContactNumber = entity.ContactNumber,
                Email = entity.Email,
                Services = entity.Services,
                OperatingHours = entity.OperatingHours,
                IsActive = entity.IsActive,
                Verified = entity.Verified,
                AverageRating = entity.AverageRating,
                TotalReviews = entity.TotalReviews,
                CreatedAt = entity.CreatedAt,
                Reviews = entity.Reviews.Select(r => new FacilityReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User?.FirstName + " " + r.User?.LastName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
    }
}
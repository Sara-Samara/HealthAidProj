// Services/Implementations/RatingService.cs
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Services.Implementations
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RatingService> _logger;

        public RatingService(ApplicationDbContext context, IMapper mapper, ILogger<RatingService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<RatingDto>> GetAllRatingsAsync(RatingFilterDto filter)
        {
            try
            {
                var query = _context.Ratings
                    .Include(r => r.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.TargetType))
                {
                    query = query.Where(r => r.TargetType == filter.TargetType);
                }

                if (filter.TargetId.HasValue)
                {
                    query = query.Where(r => r.TargetId == filter.TargetId.Value);
                }

                if (filter.UserId.HasValue)
                {
                    query = query.Where(r => r.UserId == filter.UserId.Value);
                }

                if (filter.MinRating.HasValue)
                {
                    query = query.Where(r => r.Value >= filter.MinRating.Value);
                }

                if (filter.MaxRating.HasValue)
                {
                    query = query.Where(r => r.Value <= filter.MaxRating.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(r => r.Date >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(r => r.Date <= filter.EndDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date" => filter.SortDesc ?
                        query.OrderByDescending(r => r.Date) : query.OrderBy(r => r.Date),
                    "value" => filter.SortDesc ?
                        query.OrderByDescending(r => r.Value) : query.OrderBy(r => r.Value),
                    "user" => filter.SortDesc ?
                        query.OrderByDescending(r => r.User.FirstName) : query.OrderBy(r => r.User.FirstName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(r => r.RatingId) : query.OrderBy(r => r.RatingId)
                };

                var totalCount = await query.CountAsync();
                var ratings = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(r => new RatingDto
                    {
                        RatingId = r.RatingId,
                        TargetType = r.TargetType,
                        TargetId = r.TargetId,
                        Value = r.Value,
                        Comment = r.Comment,
                        Date = r.Date,
                        UserId = r.UserId,
                        UserName = $"{r.User.FirstName} {r.User.LastName}",
                        UserEmail = r.User.Email,
                        TargetName = GetTargetName(r.TargetType, r.TargetId)
                    })
                    .ToListAsync();

                // تمرير القيم عبر الـ Constructor
                return new PagedResult<RatingDto>(ratings, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings with filter");
                throw;
            }
        }

        public async Task<RatingDto?> GetRatingByIdAsync(int id)
        {
            var rating = await _context.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null) return null;

            return new RatingDto
            {
                RatingId = rating.RatingId,
                TargetType = rating.TargetType,
                TargetId = rating.TargetId,
                Value = rating.Value,
                Comment = rating.Comment,
                Date = rating.Date,
                UserId = rating.UserId,
                UserName = $"{rating.User.FirstName} {rating.User.LastName}",
                UserEmail = rating.User.Email,
                TargetName = GetTargetName(rating.TargetType, rating.TargetId)
            };
        }

        public async Task<RatingDto> CreateRatingAsync(CreateRatingDto createRatingDto)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(createRatingDto.UserId);
            if (user == null)
                throw new ArgumentException($"User with ID {createRatingDto.UserId} not found");

            // Check if target exists
            var targetExists = await CheckTargetExistsAsync(createRatingDto.TargetType, createRatingDto.TargetId);
            if (!targetExists)
                throw new ArgumentException($"Target '{createRatingDto.TargetType}' with ID {createRatingDto.TargetId} not found");

            // Check if user has already rated this target
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == createRatingDto.UserId &&
                                         r.TargetType == createRatingDto.TargetType &&
                                         r.TargetId == createRatingDto.TargetId);

            if (existingRating != null)
                throw new InvalidOperationException("User has already rated this target");

            var rating = new Rating
            {
                TargetType = createRatingDto.TargetType,
                TargetId = createRatingDto.TargetId,
                Value = createRatingDto.Value,
                Comment = createRatingDto.Comment ?? string.Empty,
                Date = DateTime.UtcNow,
                UserId = createRatingDto.UserId
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rating created by user {UserId} for {TargetType} {TargetId}",
                createRatingDto.UserId, createRatingDto.TargetType, createRatingDto.TargetId);

            return new RatingDto
            {
                RatingId = rating.RatingId,
                TargetType = rating.TargetType,
                TargetId = rating.TargetId,
                Value = rating.Value,
                Comment = rating.Comment,
                Date = rating.Date,
                UserId = rating.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                UserEmail = user.Email,
                TargetName = GetTargetName(rating.TargetType, rating.TargetId)
            };
        }

        public async Task<RatingDto?> UpdateRatingAsync(int id, UpdateRatingDto updateRatingDto)
        {
            var rating = await _context.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RatingId == id);

            if (rating == null) return null;

            rating.Value = updateRatingDto.Value;
            rating.Comment = updateRatingDto.Comment ?? rating.Comment;
            rating.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rating {RatingId} updated", id);

            return new RatingDto
            {
                RatingId = rating.RatingId,
                TargetType = rating.TargetType,
                TargetId = rating.TargetId,
                Value = rating.Value,
                Comment = rating.Comment,
                Date = rating.Date,
                UserId = rating.UserId,
                UserName = $"{rating.User.FirstName} {rating.User.LastName}",
                UserEmail = rating.User.Email,
                TargetName = GetTargetName(rating.TargetType, rating.TargetId)
            };
        }

        public async Task<bool> DeleteRatingAsync(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null) return false;

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rating {RatingId} deleted", id);
            return true;
        }

        public async Task<bool> DeleteRatingsByUserAsync(int userId)
        {
            var userRatings = await _context.Ratings
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (!userRatings.Any()) return false;

            _context.Ratings.RemoveRange(userRatings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} ratings for user {UserId}", userRatings.Count, userId);
            return true;
        }

        public async Task<IEnumerable<RatingDto>> GetRatingsByTargetAsync(string targetType, int targetId)
        {
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .Where(r => r.TargetType == targetType && r.TargetId == targetId)
                .OrderByDescending(r => r.Date)
                .Select(r => new RatingDto
                {
                    RatingId = r.RatingId,
                    TargetType = r.TargetType,
                    TargetId = r.TargetId,
                    Value = r.Value,
                    Comment = r.Comment,
                    Date = r.Date,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    UserEmail = r.User.Email,
                    TargetName = GetTargetName(r.TargetType, r.TargetId)
                })
                .ToListAsync();

            return ratings;
        }

        public async Task<AverageRatingDto> GetAverageRatingAsync(string targetType, int targetId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.TargetType == targetType && r.TargetId == targetId)
                .ToListAsync();

            if (!ratings.Any())
                throw new ArgumentException($"No ratings found for {targetType} with ID {targetId}");

            var average = ratings.Average(r => r.Value);
            var distribution = ratings
                .GroupBy(r => r.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return new AverageRatingDto
            {
                TargetType = targetType,
                TargetId = targetId,
                Average = Math.Round(average, 2),
                TotalRatings = ratings.Count,
                Distribution = distribution,
                TargetName = await GetTargetNameAsync(targetType, targetId)
            };
        }

        public async Task<RatingStatsDto> GetRatingStatsAsync()
        {
            var totalRatings = await _context.Ratings.CountAsync();
            var averageRating = await _context.Ratings.AverageAsync(r => (double)r.Value);
            var ratingDistribution = await _context.Ratings
                .GroupBy(r => r.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var fiveStarRatings = await _context.Ratings.CountAsync(r => r.Value == 5);
            var oneStarRatings = await _context.Ratings.CountAsync(r => r.Value == 1);
            var recentRatings = await _context.Ratings
                .CountAsync(r => r.Date >= DateTime.UtcNow.AddDays(-30));

            return new RatingStatsDto
            {
                TotalRatings = totalRatings,
                AverageRating = Math.Round(averageRating, 2),
                RatingDistribution = ratingDistribution,
                FiveStarRatings = fiveStarRatings,
                OneStarRatings = oneStarRatings,
                RecentRatings = recentRatings
            };
        }

        public async Task<bool> HasUserRatedAsync(int userId, string targetType, int targetId)
        {
            return await _context.Ratings
                .AnyAsync(r => r.UserId == userId &&
                              r.TargetType == targetType &&
                              r.TargetId == targetId);
        }

        public async Task<IEnumerable<RatingDto>> GetRecentRatingsAsync(int count = 10)
        {
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .OrderByDescending(r => r.Date)
                .Take(count)
                .Select(r => new RatingDto
                {
                    RatingId = r.RatingId,
                    TargetType = r.TargetType,
                    TargetId = r.TargetId,
                    Value = r.Value,
                    Comment = r.Comment,
                    Date = r.Date,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    UserEmail = r.User.Email,
                    TargetName = GetTargetName(r.TargetType, r.TargetId)
                })
                .ToListAsync();

            return ratings;
        }

        // Helper methods
        private async Task<bool> CheckTargetExistsAsync(string targetType, int targetId)
        {
            return targetType.ToLower() switch
            {
                "ngo" => await _context.NGOs.AnyAsync(n => n.NGOId == targetId),
                "doctor" => await _context.Doctors.AnyAsync(d => d.DoctorId == targetId),
                "equipment" => await _context.Equipments.AnyAsync(e => e.EquipmentId == targetId),
                "service" => await _context.Services.AnyAsync(s => s.ServiceId == targetId),
                "user" => await _context.Users.AnyAsync(u => u.Id == targetId),
                _ => false
            };
        }

        private static string GetTargetName(string targetType, int targetId)
        {
            // This would be enhanced with actual database queries in a real implementation
            return $"{targetType} #{targetId}";
        }

        private async Task<string> GetTargetNameAsync(string targetType, int targetId)
        {
            return targetType.ToLower() switch
            {
                "ngo" => await _context.NGOs
                    .Where(n => n.NGOId == targetId)
                    .Select(n => n.OrganizationName)
                    .FirstOrDefaultAsync() ?? $"NGO #{targetId}",
                "doctor" => await _context.Doctors
                    .Include(d => d.User)
                    .Where(d => d.DoctorId == targetId)
                    .Select(d => $"{d.User.FirstName} {d.User.LastName}")
                    .FirstOrDefaultAsync() ?? $"Doctor #{targetId}",
                "equipment" => await _context.Equipments
                    .Where(e => e.EquipmentId == targetId)
                    .Select(e => e.Name)
                    .FirstOrDefaultAsync() ?? $"Equipment #{targetId}",
                _ => $"{targetType} #{targetId}"
            };
        }
    }
}
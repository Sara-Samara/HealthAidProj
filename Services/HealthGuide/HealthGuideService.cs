// Services/Implementations/HealthGuideService.cs
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.HealthGuides;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace HealthAidAPI.Services.Implementations
{
    public class HealthGuideService : IHealthGuideService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<HealthGuideService> _logger;

        public HealthGuideService(ApplicationDbContext context, IMapper mapper, ILogger<HealthGuideService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<HealthGuideDto>> GetHealthGuidesAsync(HealthGuideFilterDto filter)
        {
            try
            {
                var query = _context.HealthGuides
                    .Include(hg => hg.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(hg =>
                        hg.Title.Contains(filter.Search) ||
                        hg.Content.Contains(filter.Search) ||
                        hg.Summary != null && hg.Summary.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(hg => hg.Category == filter.Category);

                if (!string.IsNullOrEmpty(filter.Language))
                    query = query.Where(hg => hg.Language == filter.Language);

                if (filter.IsPublished.HasValue)
                    query = query.Where(hg => hg.IsPublished == filter.IsPublished.Value);

                if (filter.UserId.HasValue)
                    query = query.Where(hg => hg.UserId == filter.UserId.Value);

                if (filter.CreatedFrom.HasValue)
                    query = query.Where(hg => hg.CreatedAt >= filter.CreatedFrom.Value);

                if (filter.CreatedTo.HasValue)
                    query = query.Where(hg => hg.CreatedAt <= filter.CreatedTo.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "title" => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.Title) : query.OrderBy(hg => hg.Title),
                    "category" => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.Category) : query.OrderBy(hg => hg.Category),
                    "views" => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.ViewCount) : query.OrderBy(hg => hg.ViewCount),
                    "likes" => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.LikeCount) : query.OrderBy(hg => hg.LikeCount),
                    "createdat" => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.CreatedAt) : query.OrderBy(hg => hg.CreatedAt),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(hg => hg.HealthGuideId) : query.OrderBy(hg => hg.HealthGuideId)
                };

                var totalCount = await query.CountAsync();
                var healthGuides = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(hg => new HealthGuideDto
                    {
                        HealthGuideId = hg.HealthGuideId,
                        Title = hg.Title,
                        Category = hg.Category,
                        Content = hg.Content,
                        Language = hg.Language,
                        Summary = hg.Summary,
                        UserId = hg.UserId,
                        AuthorName = hg.User != null ? $"{hg.User.FirstName} {hg.User.LastName}" : "System",
                        CreatedAt = hg.CreatedAt,
                        UpdatedAt = hg.UpdatedAt,
                        IsPublished = hg.IsPublished,
                        ViewCount = hg.ViewCount,
                        LikeCount = hg.LikeCount,
                        ReadingTime = CalculateReadingTime(hg.Content),
                        TruncatedContent = hg.Content.Length > 200 ?
                            hg.Content.Substring(0, 200) + "..." : hg.Content
                    })
                    .ToListAsync();

                return new PagedResult<HealthGuideDto>(healthGuides, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health guides with filter");
                throw;
            }
        }

        public async Task<HealthGuideDto?> GetHealthGuideByIdAsync(int id)
        {
            var healthGuide = await _context.HealthGuides
                .Include(hg => hg.User)
                .FirstOrDefaultAsync(hg => hg.HealthGuideId == id);

            if (healthGuide == null) return null;

            // Increment view count
            healthGuide.ViewCount++;
            await _context.SaveChangesAsync();

            return new HealthGuideDto
            {
                HealthGuideId = healthGuide.HealthGuideId,
                Title = healthGuide.Title,
                Category = healthGuide.Category,
                Content = healthGuide.Content,
                Language = healthGuide.Language,
                Summary = healthGuide.Summary,
                UserId = healthGuide.UserId,
                AuthorName = healthGuide.User != null ?
                    $"{healthGuide.User.FirstName} {healthGuide.User.LastName}" : "System",
                CreatedAt = healthGuide.CreatedAt,
                UpdatedAt = healthGuide.UpdatedAt,
                IsPublished = healthGuide.IsPublished,
                ViewCount = healthGuide.ViewCount,
                LikeCount = healthGuide.LikeCount,
                ReadingTime = CalculateReadingTime(healthGuide.Content),
                TruncatedContent = healthGuide.Content.Length > 200 ?
                    healthGuide.Content.Substring(0, 200) + "..." : healthGuide.Content
            };
        }

        public async Task<HealthGuideDto> CreateHealthGuideAsync(CreateHealthGuideDto healthGuideDto)
        {
            // Validate user if provided
            if (healthGuideDto.UserId.HasValue)
            {
                var user = await _context.Users.FindAsync(healthGuideDto.UserId.Value);
                if (user == null)
                    throw new ArgumentException($"User with ID {healthGuideDto.UserId} not found");
            }

            var healthGuide = new HealthGuide
            {
                Title = healthGuideDto.Title,
                Category = healthGuideDto.Category,
                Content = healthGuideDto.Content,
                Language = healthGuideDto.Language,
                Summary = healthGuideDto.Summary,
                UserId = healthGuideDto.UserId,
                IsPublished = healthGuideDto.IsPublished,
                CreatedAt = DateTime.UtcNow
            };

            _context.HealthGuides.Add(healthGuide);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Health guide created: {Title} with ID {HealthGuideId}",
                healthGuideDto.Title, healthGuide.HealthGuideId);

            return await GetHealthGuideByIdAsync(healthGuide.HealthGuideId) ??
                throw new InvalidOperationException("Failed to retrieve created health guide");
        }

        public async Task<HealthGuideDto?> UpdateHealthGuideAsync(int id, UpdateHealthGuideDto healthGuideDto)
        {
            var healthGuide = await _context.HealthGuides
                .Include(hg => hg.User)
                .FirstOrDefaultAsync(hg => hg.HealthGuideId == id);

            if (healthGuide == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(healthGuideDto.Title))
                healthGuide.Title = healthGuideDto.Title;

            if (!string.IsNullOrEmpty(healthGuideDto.Category))
                healthGuide.Category = healthGuideDto.Category;

            if (!string.IsNullOrEmpty(healthGuideDto.Content))
                healthGuide.Content = healthGuideDto.Content;

            if (!string.IsNullOrEmpty(healthGuideDto.Language))
                healthGuide.Language = healthGuideDto.Language;

            if (healthGuideDto.Summary != null)
                healthGuide.Summary = healthGuideDto.Summary;

            if (healthGuideDto.IsPublished.HasValue)
                healthGuide.IsPublished = healthGuideDto.IsPublished.Value;

            healthGuide.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Health guide updated: {HealthGuideId}", id);

            return new HealthGuideDto
            {
                HealthGuideId = healthGuide.HealthGuideId,
                Title = healthGuide.Title,
                Category = healthGuide.Category,
                Content = healthGuide.Content,
                Language = healthGuide.Language,
                Summary = healthGuide.Summary,
                UserId = healthGuide.UserId,
                AuthorName = healthGuide.User != null ?
                    $"{healthGuide.User.FirstName} {healthGuide.User.LastName}" : "System",
                CreatedAt = healthGuide.CreatedAt,
                UpdatedAt = healthGuide.UpdatedAt,
                IsPublished = healthGuide.IsPublished,
                ViewCount = healthGuide.ViewCount,
                LikeCount = healthGuide.LikeCount,
                ReadingTime = CalculateReadingTime(healthGuide.Content),
                TruncatedContent = healthGuide.Content.Length > 200 ?
                    healthGuide.Content.Substring(0, 200) + "..." : healthGuide.Content
            };
        }

        public async Task<bool> DeleteHealthGuideAsync(int id)
        {
            var healthGuide = await _context.HealthGuides.FindAsync(id);
            if (healthGuide == null) return false;

            _context.HealthGuides.Remove(healthGuide);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Health guide deleted: {HealthGuideId}", id);
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var healthGuide = await _context.HealthGuides.FindAsync(id);
            if (healthGuide == null) return false;

            healthGuide.ViewCount++;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IncrementLikeCountAsync(int id)
        {
            var healthGuide = await _context.HealthGuides.FindAsync(id);
            if (healthGuide == null) return false;

            healthGuide.LikeCount++;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Like count incremented for health guide: {HealthGuideId}", id);
            return true;
        }

        public async Task<bool> TogglePublishStatusAsync(int id)
        {
            var healthGuide = await _context.HealthGuides.FindAsync(id);
            if (healthGuide == null) return false;

            healthGuide.IsPublished = !healthGuide.IsPublished;
            healthGuide.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Health guide {HealthGuideId} publish status toggled to: {Status}",
                id, healthGuide.IsPublished);
            return true;
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.HealthGuides
                .Select(hg => hg.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetLanguagesAsync()
        {
            return await _context.HealthGuides
                .Select(hg => hg.Language)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }

        public async Task<HealthGuideStatsDto> GetHealthGuideStatsAsync()
        {
            var totalGuides = await _context.HealthGuides.CountAsync();
            var publishedGuides = await _context.HealthGuides.CountAsync(hg => hg.IsPublished);
            var totalViews = await _context.HealthGuides.SumAsync(hg => hg.ViewCount);
            var totalLikes = await _context.HealthGuides.SumAsync(hg => hg.LikeCount);

            var categoriesCount = await _context.HealthGuides
                .GroupBy(hg => hg.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var languagesCount = await _context.HealthGuides
                .GroupBy(hg => hg.Language)
                .Select(g => new { Language = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Language, x => x.Count);

            var popularGuides = await _context.HealthGuides
                .Where(hg => hg.IsPublished)
                .OrderByDescending(hg => hg.ViewCount)
                .ThenByDescending(hg => hg.LikeCount)
                .Take(5)
                .Select(hg => new PopularGuideDto
                {
                    HealthGuideId = hg.HealthGuideId,
                    Title = hg.Title,
                    Category = hg.Category,
                    ViewCount = hg.ViewCount,
                    LikeCount = hg.LikeCount
                })
                .ToListAsync();

            return new HealthGuideStatsDto
            {
                TotalGuides = totalGuides,
                PublishedGuides = publishedGuides,
                TotalViews = totalViews,
                TotalLikes = totalLikes,
                CategoriesCount = categoriesCount,
                LanguagesCount = languagesCount,
                PopularGuides = popularGuides
            };
        }

        public async Task<IEnumerable<HealthGuideDto>> GetPopularGuidesAsync(int count = 5)
        {
            var guides = await _context.HealthGuides
                .Include(hg => hg.User)
                .Where(hg => hg.IsPublished)
                .OrderByDescending(hg => hg.ViewCount)
                .ThenByDescending(hg => hg.LikeCount)
                .Take(count)
                .Select(hg => new HealthGuideDto
                {
                    HealthGuideId = hg.HealthGuideId,
                    Title = hg.Title,
                    Category = hg.Category,
                    Content = hg.Content,
                    Language = hg.Language,
                    Summary = hg.Summary,
                    UserId = hg.UserId,
                    AuthorName = hg.User != null ? $"{hg.User.FirstName} {hg.User.LastName}" : "System",
                    CreatedAt = hg.CreatedAt,
                    UpdatedAt = hg.UpdatedAt,
                    IsPublished = hg.IsPublished,
                    ViewCount = hg.ViewCount,
                    LikeCount = hg.LikeCount,
                    ReadingTime = CalculateReadingTime(hg.Content),
                    TruncatedContent = hg.Content.Length > 200 ?
                        hg.Content.Substring(0, 200) + "..." : hg.Content
                })
                .ToListAsync();

            return guides;
        }

        public async Task<IEnumerable<HealthGuideDto>> GetRelatedGuidesAsync(int guideId, int count = 3)
        {
            var currentGuide = await _context.HealthGuides.FindAsync(guideId);
            if (currentGuide == null)
                return Enumerable.Empty<HealthGuideDto>();

            var relatedGuides = await _context.HealthGuides
                .Include(hg => hg.User)
                .Where(hg => hg.HealthGuideId != guideId &&
                           hg.Category == currentGuide.Category &&
                           hg.IsPublished)
                .OrderByDescending(hg => hg.ViewCount)
                .ThenByDescending(hg => hg.LikeCount)
                .Take(count)
                .Select(hg => new HealthGuideDto
                {
                    HealthGuideId = hg.HealthGuideId,
                    Title = hg.Title,
                    Category = hg.Category,
                    Content = hg.Content,
                    Language = hg.Language,
                    Summary = hg.Summary,
                    UserId = hg.UserId,
                    AuthorName = hg.User != null ? $"{hg.User.FirstName} {hg.User.LastName}" : "System",
                    CreatedAt = hg.CreatedAt,
                    UpdatedAt = hg.UpdatedAt,
                    IsPublished = hg.IsPublished,
                    ViewCount = hg.ViewCount,
                    LikeCount = hg.LikeCount,
                    ReadingTime = CalculateReadingTime(hg.Content),
                    TruncatedContent = hg.Content.Length > 200 ?
                        hg.Content.Substring(0, 200) + "..." : hg.Content
                })
                .ToListAsync();

            return relatedGuides;
        }

        private static int CalculateReadingTime(string content)
        {
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling(wordCount / 200.0); // 200 words per minute
        }
    }
}
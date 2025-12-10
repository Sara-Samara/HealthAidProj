// Services/Implementations/PublicAlertService.cs
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Prescriptions;
using HealthAidAPI.DTOs.PublicAlerts;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Services.Implementations
{
    public class PublicAlertService : IPublicAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PublicAlertService> _logger;

        public PublicAlertService(ApplicationDbContext context, IMapper mapper, ILogger<PublicAlertService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<PublicAlertDto>> GetAllAlertsAsync(PublicAlertFilterDto filter)
        {
            try
            {
                var query = _context.PublicAlerts
                    .Include(a => a.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Region))
                {
                    query = query.Where(a => a.Region.Contains(filter.Region));
                }

                if (!string.IsNullOrEmpty(filter.AlertType))
                {
                    query = query.Where(a => a.AlertType == filter.AlertType);
                }

                if (!string.IsNullOrEmpty(filter.Severity))
                {
                    query = query.Where(a => a.Severity == filter.Severity);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(a => a.IsActive == filter.IsActive.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(a => a.DatePosted >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(a => a.DatePosted <= filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(a =>
                        a.Title.Contains(filter.Search) ||
                        a.Description.Contains(filter.Search) ||
                        a.Region.Contains(filter.Search));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "dateposted" => filter.SortDesc ?
                        query.OrderByDescending(a => a.DatePosted) : query.OrderBy(a => a.DatePosted),
                    "severity" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Severity) : query.OrderBy(a => a.Severity),
                    "region" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Region) : query.OrderBy(a => a.Region),
                    "title" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(a => a.DatePosted) : query.OrderBy(a => a.DatePosted)
                };

                var totalCount = await query.CountAsync();
                var alerts = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new PublicAlertDto
                    {
                        AlertId = a.PublicAlertId,
                        Title = a.Title,
                        Description = a.Description,
                        Region = a.Region,
                        AlertType = a.AlertType,
                        DatePosted = a.DatePosted,
                        UserId = a.UserId,
                        PostedBy = $"{a.User.FirstName} {a.User.LastName}",
                        UserRole = a.User.Role,
                        TimeAgo = GetTimeAgo(a.DatePosted),
                        Severity = a.Severity,
                        IsActive = a.IsActive
                    })
                    .ToListAsync();

                return new PagedResult<PublicAlertDto>(alerts, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public alerts with filter");
                throw;
            }
        }

        public async Task<PublicAlertDto?> GetAlertByIdAsync(int id)
        {
            var alert = await _context.PublicAlerts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.PublicAlertId == id);

            if (alert == null) return null;

            return new PublicAlertDto
            {
                AlertId = alert.PublicAlertId,
                Title = alert.Title,
                Description = alert.Description,
                Region = alert.Region,
                AlertType = alert.AlertType,
                DatePosted = alert.DatePosted,
                UserId = alert.UserId,
                PostedBy = $"{alert.User.FirstName} {alert.User.LastName}",
                UserRole = alert.User.Role,
                TimeAgo = GetTimeAgo(alert.DatePosted),
                Severity = alert.Severity,
                IsActive = alert.IsActive
            };
        }

        public async Task<PublicAlertDto> CreateAlertAsync(CreatePublicAlertDto createAlertDto)
        {
            var user = await _context.Users.FindAsync(createAlertDto.UserId);
            if (user == null)
                throw new ArgumentException($"User with ID {createAlertDto.UserId} not found");

            var alert = new PublicAlert
            {
                Title = createAlertDto.Title,
                Description = createAlertDto.Description,
                Region = createAlertDto.Region,
                AlertType = createAlertDto.AlertType,
                DatePosted = DateTime.UtcNow,
                UserId = createAlertDto.UserId,
                Severity = createAlertDto.Severity,
                MoreInfoUrl = createAlertDto.MoreInfoUrl,
                ExpiryDate = createAlertDto.ExpiryDate,
                IsActive = true
            };

            _context.PublicAlerts.Add(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Public alert created: {AlertTitle} by user {UserId}",
                alert.Title, alert.UserId);

            return new PublicAlertDto
            {
                AlertId = alert.PublicAlertId,
                Title = alert.Title,
                Description = alert.Description,
                Region = alert.Region,
                AlertType = alert.AlertType,
                DatePosted = alert.DatePosted,
                UserId = alert.UserId,
                PostedBy = $"{user.FirstName} {user.LastName}",
                UserRole = user.Role,
                TimeAgo = GetTimeAgo(alert.DatePosted),
                Severity = alert.Severity,
                IsActive = alert.IsActive
            };
        }

        public async Task<PublicAlertDto?> UpdateAlertAsync(int id, UpdatePublicAlertDto updateAlertDto)
        {
            var alert = await _context.PublicAlerts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.PublicAlertId == id);

            if (alert == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateAlertDto.Title))
                alert.Title = updateAlertDto.Title;

            if (!string.IsNullOrEmpty(updateAlertDto.Description))
                alert.Description = updateAlertDto.Description;

            if (!string.IsNullOrEmpty(updateAlertDto.Region))
                alert.Region = updateAlertDto.Region;

            if (!string.IsNullOrEmpty(updateAlertDto.AlertType))
                alert.AlertType = updateAlertDto.AlertType;

            if (!string.IsNullOrEmpty(updateAlertDto.Severity))
                alert.Severity = updateAlertDto.Severity;

            if (updateAlertDto.MoreInfoUrl != null)
                alert.MoreInfoUrl = updateAlertDto.MoreInfoUrl;

            if (updateAlertDto.ExpiryDate.HasValue)
                alert.ExpiryDate = updateAlertDto.ExpiryDate.Value;

            if (updateAlertDto.IsActive.HasValue)
                alert.IsActive = updateAlertDto.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Public alert updated: {AlertId}", id);

            return new PublicAlertDto
            {
                AlertId = alert.PublicAlertId,
                Title = alert.Title,
                Description = alert.Description,
                Region = alert.Region,
                AlertType = alert.AlertType,
                DatePosted = alert.DatePosted,
                UserId = alert.UserId,
                PostedBy = $"{alert.User.FirstName} {alert.User.LastName}",
                UserRole = alert.User.Role,
                TimeAgo = GetTimeAgo(alert.DatePosted),
                Severity = alert.Severity,
                IsActive = alert.IsActive
            };
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            var alert = await _context.PublicAlerts.FindAsync(id);
            if (alert == null) return false;

            _context.PublicAlerts.Remove(alert);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Public alert deleted: {AlertId} - {Title}", id, alert.Title);
            return true;
        }

        public async Task<bool> DeleteAllAlertsAsync()
        {
            var alerts = await _context.PublicAlerts.ToListAsync();
            if (!alerts.Any()) return false;

            _context.PublicAlerts.RemoveRange(alerts);
            await _context.SaveChangesAsync();

            _logger.LogInformation("All {Count} public alerts deleted", alerts.Count);
            return true;
        }

        public async Task<IEnumerable<PublicAlertDto>> GetRecentAlertsAsync(int count = 5)
        {
            var alerts = await _context.PublicAlerts
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.DatePosted)
                .Take(count)
                .Select(a => new PublicAlertDto
                {
                    AlertId = a.PublicAlertId,
                    Title = a.Title,
                    Description = a.Description,
                    Region = a.Region,
                    AlertType = a.AlertType,
                    DatePosted = a.DatePosted,
                    UserId = a.UserId,
                    PostedBy = $"{a.User.FirstName} {a.User.LastName}",
                    UserRole = a.User.Role,
                    TimeAgo = GetTimeAgo(a.DatePosted),
                    Severity = a.Severity,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return alerts;
        }

        public async Task<IEnumerable<PublicAlertDto>> GetAlertsByUserAsync(int userId)
        {
            var alerts = await _context.PublicAlerts
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.DatePosted)
                .Select(a => new PublicAlertDto
                {
                    AlertId = a.PublicAlertId,
                    Title = a.Title,
                    Description = a.Description,
                    Region = a.Region,
                    AlertType = a.AlertType,
                    DatePosted = a.DatePosted,
                    UserId = a.UserId,
                    PostedBy = $"{a.User.FirstName} {a.User.LastName}",
                    UserRole = a.User.Role,
                    TimeAgo = GetTimeAgo(a.DatePosted),
                    Severity = a.Severity,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return alerts;
        }

        public async Task<AlertStatsDto> GetAlertStatsAsync()
        {
            var totalAlerts = await _context.PublicAlerts.CountAsync();
            var activeAlerts = await _context.PublicAlerts.CountAsync(a => a.IsActive);
            var criticalAlerts = await _context.PublicAlerts.CountAsync(a => a.Severity == "Critical" && a.IsActive);

            var alertsByType = await _context.PublicAlerts
                .GroupBy(a => a.AlertType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            var alertsByRegion = await _context.PublicAlerts
                .GroupBy(a => a.Region)
                .Select(g => new { Region = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Region, x => x.Count);

            var alertsBySeverity = await _context.PublicAlerts
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Severity, x => x.Count);

            var todayAlerts = await _context.PublicAlerts
                .CountAsync(a => a.DatePosted.Date == DateTime.UtcNow.Date);

            return new AlertStatsDto
            {
                TotalAlerts = totalAlerts,
                ActiveAlerts = activeAlerts,
                CriticalAlerts = criticalAlerts,
                AlertsByType = alertsByType,
                AlertsByRegion = alertsByRegion,
                AlertsBySeverity = alertsBySeverity,
                TodayAlerts = todayAlerts
            };
        }

        public async Task<IEnumerable<PublicAlertDto>> GetActiveAlertsAsync()
        {
            var alerts = await _context.PublicAlerts
                .Include(a => a.User)
                .Where(a => a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > DateTime.UtcNow))
                .OrderByDescending(a => a.DatePosted)
                .Select(a => new PublicAlertDto
                {
                    AlertId = a.PublicAlertId,
                    Title = a.Title,
                    Description = a.Description,
                    Region = a.Region,
                    AlertType = a.AlertType,
                    DatePosted = a.DatePosted,
                    UserId = a.UserId,
                    PostedBy = $"{a.User.FirstName} {a.User.LastName}",
                    UserRole = a.User.Role,
                    TimeAgo = GetTimeAgo(a.DatePosted),
                    Severity = a.Severity,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return alerts;
        }

        public async Task<bool> ToggleAlertStatusAsync(int id, bool isActive)
        {
            var alert = await _context.PublicAlerts.FindAsync(id);
            if (alert == null) return false;

            alert.IsActive = isActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Alert {AlertId} status changed to {Status}", id, isActive ? "Active" : "Inactive");
            return true;
        }

        // Helper method to calculate time ago
        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalHours < 1) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalDays < 1) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";

            return date.ToString("MMM dd, yyyy");
        }
    }
}
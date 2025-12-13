using HealthAidAPI.DTOs.Analytics;

namespace HealthAidAPI.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<ConsultationAnalyticsDto> GetConsultationAnalyticsAsync(DateTime? startDate, DateTime? endDate);
        Task<UserActivityDto> LogUserActivityAsync(LogUserActivityDto activityDto);
        Task<List<UserActivityDto>> GetUserActivitiesAsync(int? userId, string? activityType);
    }
}
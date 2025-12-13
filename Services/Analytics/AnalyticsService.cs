using Microsoft.EntityFrameworkCore;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Analytics;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalDoctors = await _context.Doctors.CountAsync();

            var totalConsultations = await _context.Appointments.CountAsync();
            var completedConsultations = await _context.Appointments
                .CountAsync(a => a.Status == "Completed");

            // إجمالي التبرعات من Donors
            var totalDonationsAmount = await _context.Donors
                .SumAsync(d => d.TotalDonated);

            var totalEmergencyCases = await _context.EmergencyCases.CountAsync();
            var totalMedicalFacilities = await _context.MedicalFacilities.CountAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                TotalConsultations = totalConsultations,
                TotalDonations = await _context.Donors.CountAsync(),
                TotalDonationsAmount = totalDonationsAmount,
                TotalEmergencyCases = totalEmergencyCases,
                TotalMedicalFacilities = totalMedicalFacilities
                // RecentDonations حذفناها مؤقتاً إذا كانت تسبب مشاكل
            };
        }

        public async Task<ConsultationAnalyticsDto> GetConsultationAnalyticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Appointments.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value);

            var totalConsultations = await query.CountAsync();
            var completedConsultations = await query
                .CountAsync(a => a.Status == "Completed");
            var cancelledConsultations = await query
                .CountAsync(a => a.Status == "Cancelled");
            var pendingConsultations = await query
                .CountAsync(a => a.Status == "Pending");

            return new ConsultationAnalyticsDto
            {
                TotalConsultations = totalConsultations,
                CompletedConsultations = completedConsultations,
                CancelledConsultations = cancelledConsultations,
                PendingConsultations = pendingConsultations,
                AverageRating = 4.2, // قيمة افتراضية
                TotalRevenue = 450   // قيمة افتراضية
            };
        }

        public async Task<UserActivityDto> LogUserActivityAsync(LogUserActivityDto dto)
        {
            // بيانات وهمية للاختبار فقط
            return await Task.FromResult(new UserActivityDto
            {
                Id = 1,
                UserId = dto.UserId,
                UserName = "Test User",
                ActivityType = dto.ActivityType,
                Description = dto.Description,
                IpAddress = dto.IpAddress ?? "127.0.0.1",
                UserAgent = dto.UserAgent ?? "Swagger",
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<List<UserActivityDto>> GetUserActivitiesAsync(int? userId, string? activityType)
        {
            // بيانات وهمية للاختبار
            var activities = new List<UserActivityDto>
            {
                new UserActivityDto
                {
                    Id = 1,
                    UserId = 1,
                    UserName = "Samir Al-Khatib",
                    ActivityType = "Login",
                    Description = "User logged in successfully",
                    IpAddress = "192.168.1.100",
                    UserAgent = "Chrome",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new UserActivityDto
                {
                    Id = 2,
                    UserId = 2,
                    UserName = "Mona Masri",
                    ActivityType = "Consultation",
                    Description = "Requested pediatric consultation",
                    IpAddress = "192.168.1.101",
                    UserAgent = "Firefox",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            // فلترة البيانات
            var result = activities.AsQueryable();

            if (userId.HasValue)
                result = result.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(activityType))
                result = result.Where(a => a.ActivityType == activityType);

            return await Task.FromResult(result.ToList());
        }
    }
}
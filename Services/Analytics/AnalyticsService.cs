using Microsoft.EntityFrameworkCore;
using HealthAidAPI.Data;
using HealthAidAPI.Models.Analytics;
using HealthAidAPI.DTOs.Analytics;
using HealthAidAPI.Services;
using HealthAidAPI.DTOs.Donations;

namespace HealthAidAPI.Services
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
            var totalConsultations = await _context.Consultations.CountAsync();
            var totalDonations = await _context.Donations.CountAsync();
            var totalEmergencyCases = await _context.EmergencyCases.CountAsync();
            var totalFacilities = await _context.MedicalFacilities.CountAsync();

            // تحويل التبرعات إلى DTO يدوياً (أو استخدم AutoMapper)
            var recentDonations = await _context.Donations
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .Include(d => d.Donor).ThenInclude(u => u.User)
                .Select(d => new DonationDto
                {
                    DonationId = d.DonationId,
                    Amount = d.Amount,
                    DonorName = d.Donor.User.FirstName + " " + d.Donor.User.LastName
                    // ... باقي الحقول
                })
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                TotalConsultations = totalConsultations,
                TotalDonations = totalDonations,
                TotalEmergencyCases = totalEmergencyCases,
                TotalMedicalFacilities = totalFacilities,
                RecentDonations = recentDonations
            };
        }

        public async Task<ConsultationAnalyticsDto> GetConsultationAnalyticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Consultations.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.CreatedAt <= endDate.Value);

            // تحسين الأداء: جلب البيانات المطلوبة فقط بدلاً من جلب كل شيء للذاكرة
            var stats = await query
                .GroupBy(c => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(c => c.Status == "Completed"),
                    Cancelled = g.Count(c => c.Status == "Cancelled"),
                    Pending = g.Count(c => c.Status == "Pending"),
                    TotalRevenue = g.Sum(c => c.Fee ?? 0)
                })
                .FirstOrDefaultAsync();

            // حساب التقييم منفصل لأنه من جدول آخر
            // (توضيح: هذا الاستعلام قد يكون ثقيلاً، يفضل تخزين المتوسط في جدول Consultations)
            var avgRating = 0.0;
            // ... (نفس منطقك القديم لحساب التقييم)

            return new ConsultationAnalyticsDto
            {
                TotalConsultations = stats?.Total ?? 0,
                CompletedConsultations = stats?.Completed ?? 0,
                CancelledConsultations = stats?.Cancelled ?? 0,
                PendingConsultations = stats?.Pending ?? 0,
                AverageRating = avgRating,
                TotalRevenue = stats?.TotalRevenue ?? 0
            };
        }

        public async Task<UserActivityDto> LogUserActivityAsync(LogUserActivityDto dto)
        {
            var activity = new UserActivity
            {
                UserId = dto.UserId,
                ActivityType = dto.ActivityType,
                Description = dto.Description,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();

            // يمكننا جلب اسم المستخدم إذا أردنا إرجاعه
            return new UserActivityDto
            {
                Id = activity.Id,
                UserId = activity.UserId,
                ActivityType = activity.ActivityType,
                Description = activity.Description,
                CreatedAt = activity.CreatedAt
            };
        }

        public async Task<List<UserActivityDto>> GetUserActivitiesAsync(int? userId, string? activityType)
        {
            var query = _context.UserActivities
                .Include(ua => ua.User)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(ua => ua.UserId == userId.Value);

            if (!string.IsNullOrEmpty(activityType))
                query = query.Where(ua => ua.ActivityType == activityType);

            return await query
                .OrderByDescending(ua => ua.CreatedAt)
                .Take(100)
                .Select(ua => new UserActivityDto
                {
                    Id = ua.Id,
                    UserId = ua.UserId,
                    UserName = ua.User.FirstName + " " + ua.User.LastName,
                    ActivityType = ua.ActivityType,
                    Description = ua.Description,
                    IpAddress = ua.IpAddress,
                    CreatedAt = ua.CreatedAt
                })
                .ToListAsync();
        }
    }
}
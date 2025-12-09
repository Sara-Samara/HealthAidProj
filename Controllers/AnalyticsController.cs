using HealthAidAPI.Data;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ApplicationDbContext context, ILogger<AnalyticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/analytics/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStats>>> GetDashboardStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalPatients = await _context.Patients.CountAsync();
                var totalDoctors = await _context.Doctors.CountAsync();
                var totalConsultations = await _context.Consultations.CountAsync();
                var totalDonations = await _context.Donations.CountAsync();
                var totalEmergencyCases = await _context.EmergencyCases.CountAsync();
                var totalFacilities = await _context.MedicalFacilities.CountAsync();

                var recentDonations = await _context.Donations
                    .OrderByDescending(d => d.DonationDate)
                    .Take(5)
                    .Include(d => d.Donor)
                    .ThenInclude(d => d.User)
                    .ToListAsync();

                var stats = new DashboardStats
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

                return Ok(new ApiResponse<DashboardStats>
                {
                    Success = true,
                    Message = "Dashboard stats retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/analytics/consultations
        [HttpGet("consultations")]
        public async Task<ActionResult<ApiResponse<ConsultationAnalytics>>> GetConsultationAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.Consultations.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(c => c.CreatedAt >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(c => c.CreatedAt <= endDate.Value);

                var consultations = await query.ToListAsync();

                var analytics = new ConsultationAnalytics
                {
                    TotalConsultations = consultations.Count,
                    CompletedConsultations = consultations.Count(c => c.Status == "Completed"),
                    CancelledConsultations = consultations.Count(c => c.Status == "Cancelled"),
                    PendingConsultations = consultations.Count(c => c.Status == "Pending"),
                    AverageRating = await _context.Ratings
                    .Where(r => r.TargetType == "Consultation" && consultations.Select(c => c.ConsultationId).Contains(r.TargetId))
                    .AverageAsync(r => (double?)r.Value) ?? 0,
                    TotalRevenue = consultations.Where(c => c.Fee.HasValue).Sum(c => c.Fee.Value)
                };

                return Ok(new ApiResponse<ConsultationAnalytics>
                {
                    Success = true,
                    Message = "Consultation analytics retrieved successfully",
                    Data = analytics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation analytics");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/analytics/activities
        [HttpPost("activities")]
        public async Task<ActionResult<ApiResponse<UserActivity>>> LogUserActivity(UserActivityRequest request)
        {
            try
            {
                var activity = new UserActivity
                {
                    UserId = request.UserId,
                    ActivityType = request.ActivityType,
                    Description = request.Description,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<UserActivity>
                {
                    Success = true,
                    Message = "User activity logged successfully",
                    Data = activity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/analytics/activities
        [HttpGet("activities")]
        public async Task<ActionResult<ApiResponse<List<UserActivity>>>> GetUserActivities(
            [FromQuery] int? userId = null,
            [FromQuery] string? activityType = null)
        {
            try
            {
                var query = _context.UserActivities
                    .Include(ua => ua.User)
                    .AsQueryable();

                if (userId.HasValue)
                    query = query.Where(ua => ua.UserId == userId.Value);

                if (!string.IsNullOrEmpty(activityType))
                    query = query.Where(ua => ua.ActivityType == activityType);

                var activities = await query
                    .OrderByDescending(ua => ua.CreatedAt)
                    .Take(100)
                    .ToListAsync();

                return Ok(new ApiResponse<List<UserActivity>>
                {
                    Success = true,
                    Message = "User activities retrieved successfully",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activities");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }

    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalConsultations { get; set; }
        public int TotalDonations { get; set; }
        public int TotalEmergencyCases { get; set; }
        public int TotalMedicalFacilities { get; set; }
        public List<Donation> RecentDonations { get; set; } = new();
    }

    public class ConsultationAnalytics
    {
        public int TotalConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int CancelledConsultations { get; set; }
        public int PendingConsultations { get; set; }
        public double AverageRating { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class UserActivityRequest
    {
        public int UserId { get; set; }
        public required string ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
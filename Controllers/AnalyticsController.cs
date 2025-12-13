using HealthAidAPI.DTOs.Analytics;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    //[Authorize(Roles = "Admin,Doctor")] 
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                var stats = await _analyticsService.GetDashboardStatsAsync();
                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Message = "Dashboard stats retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("consultations")]
        public async Task<ActionResult<ApiResponse<ConsultationAnalyticsDto>>> GetConsultationAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // التحقق من صحة التواريخ
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Start date cannot be after end date"
                    });
                }

                var analytics = await _analyticsService.GetConsultationAnalyticsAsync(startDate, endDate);
                return Ok(new ApiResponse<ConsultationAnalyticsDto>
                {
                    Success = true,
                    Message = "Consultation analytics retrieved successfully",
                    Data = analytics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation analytics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("activities")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<UserActivityDto>>> LogUserActivity([FromBody] LogUserActivityDto activityDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                var activity = await _analyticsService.LogUserActivityAsync(activityDto);
                return Ok(new ApiResponse<UserActivityDto>
                {
                    Success = true,
                    Message = "User activity logged successfully",
                    Data = activity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("activities")]
        public async Task<ActionResult<ApiResponse<List<UserActivityDto>>>> GetUserActivities(
            [FromQuery] int? userId = null,
            [FromQuery] string? activityType = null)
        {
            try
            {
                var activities = await _analyticsService.GetUserActivitiesAsync(userId, activityType);
                return Ok(new ApiResponse<List<UserActivityDto>>
                {
                    Success = true,
                    Message = "User activities retrieved successfully",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activities");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Analytics service is healthy",
                Data = new
                {
                    Service = "Analytics API",
                    Status = "Running",
                    Timestamp = DateTime.UtcNow
                }
            });
        }
    }
}
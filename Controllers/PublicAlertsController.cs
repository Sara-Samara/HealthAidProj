// Controllers/PublicAlertsController.cs
using HealthAidAPI.DTOs.PublicAlerts;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PublicAlertsController : ControllerBase
    {
        private readonly IPublicAlertService _alertService;
        private readonly ILogger<PublicAlertsController> _logger;

        public PublicAlertsController(IPublicAlertService alertService, ILogger<PublicAlertsController> logger)
        {
            _alertService = alertService;
            _logger = logger;
        }

        /// <summary>
        /// Get all public alerts with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<PublicAlertDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<PublicAlertDto>>> GetAlerts([FromQuery] PublicAlertFilterDto filter)
        {
            try
            {
                var result = await _alertService.GetAllAlertsAsync(filter);
                return Ok(new ApiResponse<PagedResult<PublicAlertDto>>
                {
                    Success = true,
                    Message = "Public alerts retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public alerts");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving public alerts"
                });
            }
        }

        /// <summary>
        /// Get alert by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PublicAlertDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PublicAlertDto>> GetAlert(int id)
        {
            try
            {
                var alert = await _alertService.GetAlertByIdAsync(id);
                if (alert == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Alert with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<PublicAlertDto>
                {
                    Success = true,
                    Message = "Alert retrieved successfully",
                    Data = alert
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert {AlertId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the alert"
                });
            }
        }

        /// <summary>
        /// Create a new public alert
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HealthWorker,NGOManager")]
        [ProducesResponseType(typeof(PublicAlertDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PublicAlertDto>> CreateAlert(CreatePublicAlertDto createAlertDto)
        {
            try
            {
                var alert = await _alertService.CreateAlertAsync(createAlertDto);
                return CreatedAtAction(nameof(GetAlert), new { id = alert.AlertId },
                    new ApiResponse<PublicAlertDto>
                    {
                        Success = true,
                        Message = "Public alert created successfully",
                        Data = alert
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating public alert");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the alert"
                });
            }
        }

        /// <summary>
        /// Update an alert
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,HealthWorker,NGOManager")]
        [ProducesResponseType(typeof(PublicAlertDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PublicAlertDto>> UpdateAlert(int id, UpdatePublicAlertDto updateAlertDto)
        {
            try
            {
                var alert = await _alertService.UpdateAlertAsync(id, updateAlertDto);
                if (alert == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Alert with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<PublicAlertDto>
                {
                    Success = true,
                    Message = "Alert updated successfully",
                    Data = alert
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert {AlertId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the alert"
                });
            }
        }

        /// <summary>
        /// Delete an alert
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,HealthWorker")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            try
            {
                var result = await _alertService.DeleteAlertAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Alert with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Alert deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert {AlertId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the alert"
                });
            }
        }

        /// <summary>
        /// Get recent alerts
        /// </summary>
        [HttpGet("recent")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PublicAlertDto>), 200)]
        public async Task<ActionResult<IEnumerable<PublicAlertDto>>> GetRecentAlerts([FromQuery] int count = 5)
        {
            try
            {
                var alerts = await _alertService.GetRecentAlertsAsync(count);
                return Ok(new ApiResponse<IEnumerable<PublicAlertDto>>
                {
                    Success = true,
                    Message = $"Recent {count} alerts retrieved successfully",
                    Data = alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent alerts");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving recent alerts"
                });
            }
        }

        /// <summary>
        /// Get alerts by user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<PublicAlertDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<PublicAlertDto>>> GetAlertsByUser(int userId)
        {
            try
            {
                var alerts = await _alertService.GetAlertsByUserAsync(userId);
                if (!alerts.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No alerts found for user ID {userId}"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<PublicAlertDto>>
                {
                    Success = true,
                    Message = $"Alerts for user {userId} retrieved successfully",
                    Data = alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alerts for user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user alerts"
                });
            }
        }

        /// <summary>
        /// Get active alerts
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PublicAlertDto>), 200)]
        public async Task<ActionResult<IEnumerable<PublicAlertDto>>> GetActiveAlerts()
        {
            try
            {
                var alerts = await _alertService.GetActiveAlertsAsync();
                return Ok(new ApiResponse<IEnumerable<PublicAlertDto>>
                {
                    Success = true,
                    Message = "Active alerts retrieved successfully",
                    Data = alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active alerts");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving active alerts"
                });
            }
        }

        /// <summary>
        /// Get alert statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,HealthWorker,NGOManager")]
        [ProducesResponseType(typeof(AlertStatsDto), 200)]
        public async Task<ActionResult<AlertStatsDto>> GetAlertStats()
        {
            try
            {
                var stats = await _alertService.GetAlertStatsAsync();
                return Ok(new ApiResponse<AlertStatsDto>
                {
                    Success = true,
                    Message = "Alert statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving alert statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving alert statistics"
                });
            }
        }

        /// <summary>
        /// Toggle alert status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,HealthWorker")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleAlertStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _alertService.ToggleAlertStatusAsync(id, isActive);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Alert with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Alert status updated to {(isActive ? "Active" : "Inactive")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling alert status for {AlertId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating alert status"
                });
            }
        }

        /// <summary>
        /// Delete all alerts (Admin only)
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteAllAlerts()
        {
            try
            {
                var result = await _alertService.DeleteAllAlertsAsync();
                if (!result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No alerts found to delete"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "All alerts deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all alerts");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting all alerts"
                });
            }
        }
    }
}
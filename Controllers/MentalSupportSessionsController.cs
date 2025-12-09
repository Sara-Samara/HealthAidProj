// Controllers/MentalSupportSessionsController.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;
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
    public class MentalSupportSessionsController : ControllerBase
    {
        private readonly IMentalSupportSessionService _sessionService;
        private readonly ILogger<MentalSupportSessionsController> _logger;

        public MentalSupportSessionsController(IMentalSupportSessionService sessionService, ILogger<MentalSupportSessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all mental support sessions with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MentalSupportSessionDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<MentalSupportSessionDto>>> GetSessions([FromQuery] MentalSupportSessionFilterDto filter)
        {
            try
            {
                var result = await _sessionService.GetSessionsAsync(filter);
                return Ok(new ApiResponse<PagedResult<MentalSupportSessionDto>>
                {
                    Success = true,
                    Message = "Mental support sessions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mental support sessions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving mental support sessions"
                });
            }
        }

        /// <summary>
        /// Get mental support session by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MentalSupportSessionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MentalSupportSessionDto>> GetSession(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MentalSupportSessionDto>
                {
                    Success = true,
                    Message = "Mental support session retrieved successfully",
                    Data = session
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the mental support session"
                });
            }
        }

        /// <summary>
        /// Create a new mental support session
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(MentalSupportSessionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MentalSupportSessionDto>> CreateSession(CreateMentalSupportSessionDto sessionDto)
        {
            try
            {
                var session = await _sessionService.CreateSessionAsync(sessionDto);
                return CreatedAtAction(nameof(GetSession), new { id = session.MentalSupportSessionId },
                    new ApiResponse<MentalSupportSessionDto>
                    {
                        Success = true,
                        Message = "Mental support session created successfully",
                        Data = session
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mental support session");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the mental support session"
                });
            }
        }

        /// <summary>
        /// Update mental support session
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(MentalSupportSessionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MentalSupportSessionDto>> UpdateSession(int id, UpdateMentalSupportSessionDto sessionDto)
        {
            try
            {
                var session = await _sessionService.UpdateSessionAsync(id, sessionDto);
                if (session == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MentalSupportSessionDto>
                {
                    Success = true,
                    Message = "Mental support session updated successfully",
                    Data = session
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the mental support session"
                });
            }
        }

        /// <summary>
        /// Delete mental support session
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                var result = await _sessionService.DeleteSessionAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Mental support session deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the mental support session"
                });
            }
        }

        /// <summary>
        /// Complete a mental support session
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CompleteSession(int id, [FromBody] string? notes = null)
        {
            try
            {
                var result = await _sessionService.CompleteSessionAsync(id, notes);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Mental support session completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while completing the mental support session"
                });
            }
        }

        /// <summary>
        /// Cancel a mental support session
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelSession(int id, [FromBody] string? reason = null)
        {
            try
            {
                var result = await _sessionService.CancelSessionAsync(id, reason);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Mental support session cancelled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling the mental support session"
                });
            }
        }

        /// <summary>
        /// Get mental support session statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(MentalSupportSessionStatsDto), 200)]
        public async Task<ActionResult<MentalSupportSessionStatsDto>> GetSessionStats()
        {
            try
            {
                var stats = await _sessionService.GetSessionStatsAsync();
                return Ok(new ApiResponse<MentalSupportSessionStatsDto>
                {
                    Success = true,
                    Message = "Mental support session statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mental support session statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving mental support session statistics"
                });
            }
        }

        /// <summary>
        /// Get upcoming mental support sessions
        /// </summary>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<MentalSupportSessionDto>), 200)]
        public async Task<ActionResult<IEnumerable<MentalSupportSessionDto>>> GetUpcomingSessions([FromQuery] int days = 7)
        {
            try
            {
                var sessions = await _sessionService.GetUpcomingSessionsAsync(days);
                return Ok(new ApiResponse<IEnumerable<MentalSupportSessionDto>>
                {
                    Success = true,
                    Message = $"Upcoming mental support sessions for the next {days} days retrieved successfully",
                    Data = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming mental support sessions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving upcoming mental support sessions"
                });
            }
        }

        /// <summary>
        /// Get mental support sessions for a specific patient
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(IEnumerable<MentalSupportSessionDto>), 200)]
        public async Task<ActionResult<IEnumerable<MentalSupportSessionDto>>> GetPatientSessions(int patientId)
        {
            try
            {
                var sessions = await _sessionService.GetPatientSessionsAsync(patientId);
                return Ok(new ApiResponse<IEnumerable<MentalSupportSessionDto>>
                {
                    Success = true,
                    Message = "Patient mental support sessions retrieved successfully",
                    Data = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient mental support sessions for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving patient mental support sessions"
                });
            }
        }

        /// <summary>
        /// Get mental support sessions for a specific doctor
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<MentalSupportSessionDto>), 200)]
        public async Task<ActionResult<IEnumerable<MentalSupportSessionDto>>> GetDoctorSessions(int doctorId)
        {
            try
            {
                var sessions = await _sessionService.GetDoctorSessionsAsync(doctorId);
                return Ok(new ApiResponse<IEnumerable<MentalSupportSessionDto>>
                {
                    Success = true,
                    Message = "Doctor mental support sessions retrieved successfully",
                    Data = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor mental support sessions for doctor {DoctorId}", doctorId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving doctor mental support sessions"
                });
            }
        }

        /// <summary>
        /// Get doctor availability for scheduling
        /// </summary>
        [HttpGet("availability/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(SessionAvailabilityDto), 200)]
        public async Task<ActionResult<SessionAvailabilityDto>> GetDoctorAvailability(int doctorId, [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _sessionService.GetDoctorAvailabilityAsync(doctorId, date);
                return Ok(new ApiResponse<SessionAvailabilityDto>
                {
                    Success = true,
                    Message = "Doctor availability retrieved successfully",
                    Data = availability
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
                _logger.LogError(ex, "Error retrieving doctor availability for doctor {DoctorId} on {Date}", doctorId, date);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving doctor availability"
                });
            }
        }

        /// <summary>
        /// Reschedule a mental support session
        /// </summary>
        [HttpPost("{id}/reschedule")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RescheduleSession(int id, [FromBody] DateTime newDate)
        {
            try
            {
                var result = await _sessionService.RescheduleSessionAsync(id, newDate);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Mental support session with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Mental support session rescheduled successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling mental support session {SessionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while rescheduling the mental support session"
                });
            }
        }

        /// <summary>
        /// Get all available session types
        /// </summary>
        [HttpGet("types")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetSessionTypes()
        {
            try
            {
                var types = await _sessionService.GetSessionTypesAsync();
                return Ok(new ApiResponse<IEnumerable<string>>
                {
                    Success = true,
                    Message = "Session types retrieved successfully",
                    Data = types
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session types");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving session types"
                });
            }
        }
    }
}
// Controllers/AppointmentsController.cs
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
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all appointments with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AppointmentDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<AppointmentDto>>> GetAppointments([FromQuery] AppointmentFilterDto filter)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsAsync(filter);
                return Ok(new ApiResponse<PagedResult<AppointmentDto>>
                {
                    Success = true,
                    Message = "Appointments retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving appointments"
                });
            }
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<AppointmentDto>
                {
                    Success = true,
                    Message = "Appointment retrieved successfully",
                    Data = appointment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the appointment"
                });
            }
        }

        /// <summary>
        /// Create a new appointment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment(CreateAppointmentDto createAppointmentDto)
        {
            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);
                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId },
                    new ApiResponse<AppointmentDto>
                    {
                        Success = true,
                        Message = "Appointment created successfully",
                        Data = appointment
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
                _logger.LogError(ex, "Error creating appointment");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the appointment"
                });
            }
        }

        /// <summary>
        /// Update an appointment
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AppointmentDto>> UpdateAppointment(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            try
            {
                var appointment = await _appointmentService.UpdateAppointmentAsync(id, updateAppointmentDto);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<AppointmentDto>
                {
                    Success = true,
                    Message = "Appointment updated successfully",
                    Data = appointment
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
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the appointment"
                });
            }
        }

        /// <summary>
        /// Reschedule an appointment
        /// </summary>
        [HttpPatch("{id}/reschedule")]
        [ProducesResponseType(typeof(AppointmentDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AppointmentDto>> RescheduleAppointment(int id, RescheduleAppointmentDto rescheduleDto)
        {
            try
            {
                var appointment = await _appointmentService.RescheduleAppointmentAsync(id, rescheduleDto);
                if (appointment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<AppointmentDto>
                {
                    Success = true,
                    Message = "Appointment rescheduled successfully",
                    Data = appointment
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
                _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while rescheduling the appointment"
                });
            }
        }

        /// <summary>
        /// Cancel an appointment
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelAppointment(int id, [FromQuery] string? reason = null)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id, reason);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment canceled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while canceling the appointment"
                });
            }
        }

        /// <summary>
        /// Confirm an appointment
        /// </summary>
        [HttpPatch("{id}/confirm")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            try
            {
                var result = await _appointmentService.ConfirmAppointmentAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment confirmed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while confirming the appointment"
                });
            }
        }

        /// <summary>
        /// Complete an appointment
        /// </summary>
        [HttpPatch("{id}/complete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            try
            {
                var result = await _appointmentService.CompleteAppointmentAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while completing the appointment"
                });
            }
        }

        /// <summary>
        /// Delete an appointment
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Appointment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Appointment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the appointment"
                });
            }
        }

        /// <summary>
        /// Get appointment statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(AppointmentStatsDto), 200)]
        public async Task<ActionResult<AppointmentStatsDto>> GetAppointmentStats()
        {
            try
            {
                var stats = await _appointmentService.GetAppointmentStatsAsync();
                return Ok(new ApiResponse<AppointmentStatsDto>
                {
                    Success = true,
                    Message = "Appointment statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving appointment statistics"
                });
            }
        }

        /// <summary>
        /// Get upcoming appointments
        /// </summary>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetUpcomingAppointments([FromQuery] int days = 7)
        {
            try
            {
                var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(days);
                return Ok(new ApiResponse<IEnumerable<AppointmentDto>>
                {
                    Success = true,
                    Message = $"Upcoming appointments for the next {days} days retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving upcoming appointments"
                });
            }
        }

        /// <summary>
        /// Get doctor's appointments
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetDoctorAppointments(int doctorId, [FromQuery] DateTime? date = null)
        {
            try
            {
                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId, date);
                return Ok(new ApiResponse<IEnumerable<AppointmentDto>>
                {
                    Success = true,
                    Message = date.HasValue ?
                        $"Doctor appointments for {date.Value:yyyy-MM-dd} retrieved successfully" :
                        "Doctor appointments retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor appointments for doctor {DoctorId}", doctorId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving doctor appointments"
                });
            }
        }

        /// <summary>
        /// Get patient's appointments
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetPatientAppointments(int patientId)
        {
            try
            {
                var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId);
                return Ok(new ApiResponse<IEnumerable<AppointmentDto>>
                {
                    Success = true,
                    Message = "Patient appointments retrieved successfully",
                    Data = appointments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient appointments for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving patient appointments"
                });
            }
        }

        /// <summary>
        /// Check if time slot is available
        /// </summary>
        [HttpGet("availability")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckTimeSlotAvailability([FromQuery] int doctorId, [FromQuery] DateTime dateTime)
        {
            try
            {
                var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(doctorId, dateTime);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Time slot availability checked successfully",
                    Data = isAvailable
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking time slot availability for doctor {DoctorId}", doctorId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while checking time slot availability"
                });
            }
        }
    }
}
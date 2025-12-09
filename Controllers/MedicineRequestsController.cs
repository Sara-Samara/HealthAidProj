// Controllers/MedicineRequestsController.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Services.MedicineRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class MedicineRequestsController : ControllerBase
    {
        private readonly IMedicineRequestService _medicineRequestService;
        private readonly ILogger<MedicineRequestsController> _logger;

        public MedicineRequestsController(IMedicineRequestService medicineRequestService, ILogger<MedicineRequestsController> logger)
        {
            _medicineRequestService = medicineRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Get all medicine requests with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(PagedResult<MedicineRequestDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<MedicineRequestDto>>> GetMedicineRequests([FromQuery] MedicineRequestFilterDto filter)
        {
            try
            {
                var result = await _medicineRequestService.GetMedicineRequestsAsync(filter);
                return Ok(new ApiResponse<PagedResult<MedicineRequestDto>>
                {
                    Success = true,
                    Message = "Medicine requests retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medicine requests");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving medicine requests"
                });
            }
        }

        /// <summary>
        /// Get medicine request by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MedicineRequestDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MedicineRequestDto>> GetMedicineRequest(int id)
        {
            try
            {
                var medicineRequest = await _medicineRequestService.GetMedicineRequestByIdAsync(id);
                if (medicineRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MedicineRequestDto>
                {
                    Success = true,
                    Message = "Medicine request retrieved successfully",
                    Data = medicineRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the medicine request"
                });
            }
        }

        /// <summary>
        /// Create a new medicine request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(typeof(MedicineRequestDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MedicineRequestDto>> CreateMedicineRequest(CreateMedicineRequestDto createMedicineRequestDto)
        {
            try
            {
                var medicineRequest = await _medicineRequestService.CreateMedicineRequestAsync(createMedicineRequestDto);
                return CreatedAtAction(nameof(GetMedicineRequest), new { id = medicineRequest.MedicineRequestId },
                    new ApiResponse<MedicineRequestDto>
                    {
                        Success = true,
                        Message = "Medicine request created successfully",
                        Data = medicineRequest
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
                _logger.LogError(ex, "Error creating medicine request");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the medicine request"
                });
            }
        }

        /// <summary>
        /// Update a medicine request
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(MedicineRequestDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MedicineRequestDto>> UpdateMedicineRequest(int id, UpdateMedicineRequestDto updateMedicineRequestDto)
        {
            try
            {
                var medicineRequest = await _medicineRequestService.UpdateMedicineRequestAsync(id, updateMedicineRequestDto);
                if (medicineRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MedicineRequestDto>
                {
                    Success = true,
                    Message = "Medicine request updated successfully",
                    Data = medicineRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the medicine request"
                });
            }
        }

        /// <summary>
        /// Delete a medicine request
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMedicineRequest(int id)
        {
            try
            {
                var result = await _medicineRequestService.DeleteMedicineRequestAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Medicine request deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the medicine request"
                });
            }
        }

        /// <summary>
        /// Update medicine request status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(MedicineRequestDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MedicineRequestDto>> UpdateStatus(int id, UpdateMedicineRequestStatusDto updateStatusDto)
        {
            try
            {
                var medicineRequest = await _medicineRequestService.UpdateStatusAsync(id, updateStatusDto);
                if (medicineRequest == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MedicineRequestDto>
                {
                    Success = true,
                    Message = $"Medicine request status updated to {updateStatusDto.Status}",
                    Data = medicineRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the status"
                });
            }
        }

        /// <summary>
        /// Get medicine request statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(MedicineRequestStatsDto), 200)]
        public async Task<ActionResult<MedicineRequestStatsDto>> GetStats()
        {
            try
            {
                var stats = await _medicineRequestService.GetMedicineRequestStatsAsync();
                return Ok(new ApiResponse<MedicineRequestStatsDto>
                {
                    Success = true,
                    Message = "Medicine request statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medicine request statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics"
                });
            }
        }

        /// <summary>
        /// Get urgent medicine requests
        /// </summary>
        [HttpGet("urgent")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<MedicineRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<MedicineRequestDto>>> GetUrgentMedicineRequests()
        {
            try
            {
                var medicineRequests = await _medicineRequestService.GetUrgentMedicineRequestsAsync();
                return Ok(new ApiResponse<IEnumerable<MedicineRequestDto>>
                {
                    Success = true,
                    Message = "Urgent medicine requests retrieved successfully",
                    Data = medicineRequests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving urgent medicine requests");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving urgent medicine requests"
                });
            }
        }

        /// <summary>
        /// Get overdue medicine requests
        /// </summary>
        [HttpGet("overdue")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<MedicineRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<MedicineRequestDto>>> GetOverdueMedicineRequests()
        {
            try
            {
                var medicineRequests = await _medicineRequestService.GetOverdueMedicineRequestsAsync();
                return Ok(new ApiResponse<IEnumerable<MedicineRequestDto>>
                {
                    Success = true,
                    Message = "Overdue medicine requests retrieved successfully",
                    Data = medicineRequests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue medicine requests");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving overdue medicine requests"
                });
            }
        }

        /// <summary>
        /// Get medicine requests by patient
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<MedicineRequestDto>), 200)]
        public async Task<ActionResult<IEnumerable<MedicineRequestDto>>> GetMedicineRequestsByPatient(int patientId)
        {
            try
            {
                var medicineRequests = await _medicineRequestService.GetMedicineRequestsByPatientAsync(patientId);
                return Ok(new ApiResponse<IEnumerable<MedicineRequestDto>>
                {
                    Success = true,
                    Message = $"Medicine requests for patient {patientId} retrieved successfully",
                    Data = medicineRequests
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medicine requests for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving medicine requests"
                });
            }
        }

        /// <summary>
        /// Fulfill a medicine request
        /// </summary>
        [HttpPost("{id}/fulfill")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> FulfillMedicineRequest(int id)
        {
            try
            {
                var result = await _medicineRequestService.FulfillMedicineRequestAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Medicine request fulfilled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fulfilling medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fulfilling the medicine request"
                });
            }
        }

        /// <summary>
        /// Cancel a medicine request
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelMedicineRequest(int id)
        {
            try
            {
                var result = await _medicineRequestService.CancelMedicineRequestAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Medicine request with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Medicine request cancelled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling medicine request {MedicineRequestId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling the medicine request"
                });
            }
        }
    }
}
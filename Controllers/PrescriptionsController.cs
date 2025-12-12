// Controllers/PrescriptionsController.cs
using HealthAidAPI.DTOs.Prescriptions;
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
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(IPrescriptionService prescriptionService, ILogger<PrescriptionsController> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all prescriptions with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(PagedResult<PrescriptionDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<PrescriptionDto>>> GetPrescriptions([FromQuery] PrescriptionFilterDto filter)
        {
            try
            {
                var result = await _prescriptionService.GetPrescriptionsAsync(filter);
                return Ok(new ApiResponse<PagedResult<PrescriptionDto>>
                {
                    Success = true,
                    Message = "Prescriptions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving prescriptions"
                });
            }
        }

        /// <summary>
        /// Get prescription by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PrescriptionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PrescriptionDto>> GetPrescription(int id)
        {
            try
            {
                var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
                if (prescription == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<PrescriptionDto>
                {
                    Success = true,
                    Message = "Prescription retrieved successfully",
                    Data = prescription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the prescription"
                });
            }
        }

        /// <summary>
        /// Create a new prescription
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(PrescriptionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PrescriptionDto>> CreatePrescription(CreatePrescriptionDto prescriptionDto)
        {
            try
            {
                var prescription = await _prescriptionService.CreatePrescriptionAsync(prescriptionDto);
                return CreatedAtAction(nameof(GetPrescription), new { id = prescription.PrescriptionId },
                    new ApiResponse<PrescriptionDto>
                    {
                        Success = true,
                        Message = "Prescription created successfully",
                        Data = prescription
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
                _logger.LogError(ex, "Error creating prescription");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the prescription"
                });
            }
        }

        /// <summary>
        /// Update prescription
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(PrescriptionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PrescriptionDto>> UpdatePrescription(int id, UpdatePrescriptionDto prescriptionDto)
        {
            try
            {
                var prescription = await _prescriptionService.UpdatePrescriptionAsync(id, prescriptionDto);
                if (prescription == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<PrescriptionDto>
                {
                    Success = true,
                    Message = "Prescription updated successfully",
                    Data = prescription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the prescription"
                });
            }
        }

        /// <summary>
        /// Delete prescription
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            try
            {
                var result = await _prescriptionService.DeletePrescriptionAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Prescription deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the prescription"
                });
            }
        }

        /// <summary>
        /// Complete a prescription
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CompletePrescription(int id)
        {
            try
            {
                var result = await _prescriptionService.CompletePrescriptionAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Prescription completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while completing the prescription"
                });
            }
        }

        /// <summary>
        /// Request a prescription refill
        /// </summary>
        [HttpPost("refill-request")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RequestRefill(RefillRequestDto refillRequest)
        {
            try
            {
                var result = await _prescriptionService.RequestRefillAsync(refillRequest);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {refillRequest.PrescriptionId} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Refill request submitted successfully"
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
                _logger.LogError(ex, "Error requesting refill for prescription {PrescriptionId}", refillRequest.PrescriptionId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while requesting refill"
                });
            }
        }

        /// <summary>
        /// Approve a prescription refill
        /// </summary>
        [HttpPost("{id}/approve-refill")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ApproveRefill(int id, [FromQuery] int quantity = 1)
        {
            try
            {
                var result = await _prescriptionService.ApproveRefillAsync(id, quantity);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Prescription with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Refill approved successfully. {quantity} refill(s) granted"
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
                _logger.LogError(ex, "Error approving refill for prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while approving refill"
                });
            }
        }

        /// <summary>
        /// Get prescription statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(PrescriptionStatsDto), 200)]
        public async Task<ActionResult<PrescriptionStatsDto>> GetPrescriptionStats()
        {
            try
            {
                var stats = await _prescriptionService.GetPrescriptionStatsAsync();
                return Ok(new ApiResponse<PrescriptionStatsDto>
                {
                    Success = true,
                    Message = "Prescription statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescription statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving prescription statistics"
                });
            }
        }

        /// <summary>
        /// Get prescriptions for a specific patient
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), 200)]
        public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetPatientPrescriptions(int patientId)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);
                return Ok(new ApiResponse<IEnumerable<PrescriptionDto>>
                {
                    Success = true,
                    Message = "Patient prescriptions retrieved successfully",
                    Data = prescriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient prescriptions for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving patient prescriptions"
                });
            }
        }

        /// <summary>
        /// Get prescriptions for a specific doctor
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), 200)]
        public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetDoctorPrescriptions(int doctorId)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetDoctorPrescriptionsAsync(doctorId);
                return Ok(new ApiResponse<IEnumerable<PrescriptionDto>>
                {
                    Success = true,
                    Message = "Doctor prescriptions retrieved successfully",
                    Data = prescriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor prescriptions for doctor {DoctorId}", doctorId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving doctor prescriptions"
                });
            }
        }

        /// <summary>
        /// Get patient prescription summary
        /// </summary>
        [HttpGet("patient/{patientId}/summary")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        [ProducesResponseType(typeof(PatientPrescriptionSummaryDto), 200)]
        public async Task<ActionResult<PatientPrescriptionSummaryDto>> GetPatientPrescriptionSummary(int patientId)
        {
            try
            {
                var summary = await _prescriptionService.GetPatientPrescriptionSummaryAsync(patientId);
                return Ok(new ApiResponse<PatientPrescriptionSummaryDto>
                {
                    Success = true,
                    Message = "Patient prescription summary retrieved successfully",
                    Data = summary
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
                _logger.LogError(ex, "Error retrieving patient prescription summary for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving patient prescription summary"
                });
            }
        }

        /// <summary>
        /// Get expiring prescriptions
        /// </summary>
        [HttpGet("expiring")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), 200)]
        public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetExpiringPrescriptions([FromQuery] int days = 7)
        {
            try
            {
                var prescriptions = await _prescriptionService.GetExpiringPrescriptionsAsync(days);
                return Ok(new ApiResponse<IEnumerable<PrescriptionDto>>
                {
                    Success = true,
                    Message = $"Expiring prescriptions within {days} days retrieved successfully",
                    Data = prescriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring prescriptions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving expiring prescriptions"
                });
            }
        }

        /// <summary>
        /// Validate a prescription
        /// </summary>
        [HttpGet("{id}/validate")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<bool>> ValidatePrescription(int id)
        {
            try
            {
                var isValid = await _prescriptionService.ValidatePrescriptionAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = isValid ? "Prescription is valid" : "Prescription is not valid",
                    Data = isValid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating prescription {PrescriptionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while validating the prescription"
                });
            }
        }
    }
}
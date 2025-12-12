using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Consultations;
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
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(IConsultationService consultationService, ILogger<ConsultationsController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all consultations with filtering (Doctor, Patient, Date, Status, Search) and pagination
        /// </summary>
        /// <remarks>
        /// Use this endpoint for all filtering needs:
        /// - By Doctor: GET /api/Consultations?doctorId=5
        /// - By Patient: GET /api/Consultations?patientId=10
        /// - By Date: GET /api/Consultations?startDate=2023-01-01&endDate=2023-01-31
        /// </remarks>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(typeof(PagedResult<ConsultationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<ConsultationDto>>> GetConsultations([FromQuery] ConsultationFilterDto filter)
        {
            try
            {
                var result = await _consultationService.GetConsultationsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations");
                return BadRequest(new { message = "An error occurred while retrieving consultations" });
            }
        }

        /// <summary>
        /// Get consultation by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(typeof(ConsultationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ConsultationDto>> GetConsultation(int id)
        {
            var consultation = await _consultationService.GetConsultationByIdAsync(id);
            if (consultation == null)
                return NotFound(new { message = "Consultation not found" });

            return Ok(consultation);
        }

        /// <summary>
        /// Create a new consultation
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(ConsultationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ConsultationDto>> CreateConsultation(CreateConsultationDto consultationDto)
        {
            try
            {
                var consultation = await _consultationService.CreateConsultationAsync(consultationDto);
                return CreatedAtAction(nameof(GetConsultation), new { id = consultation.ConsultationId }, consultation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation");
                return BadRequest(new { message = "An error occurred while creating consultation" });
            }
        }

        /// <summary>
        /// Update consultation
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(ConsultationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ConsultationDto>> UpdateConsultation(int id, UpdateConsultationDto consultationDto)
        {
            try
            {
                var consultation = await _consultationService.UpdateConsultationAsync(id, consultationDto);
                if (consultation == null)
                    return NotFound(new { message = "Consultation not found" });

                return Ok(consultation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating consultation {ConsultationId}", id);
                return BadRequest(new { message = "An error occurred while updating consultation" });
            }
        }

        /// <summary>
        /// Delete consultation
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteConsultation(int id)
        {
            try
            {
                var result = await _consultationService.DeleteConsultationAsync(id);
                if (!result)
                    return NotFound(new { message = "Consultation not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consultation {ConsultationId}", id);
                return BadRequest(new { message = "An error occurred while deleting consultation" });
            }
        }

        /// <summary>
        /// Update consultation status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateConsultationStatus(int id, [FromBody] UpdateConsultationStatusDto statusDto)
        {
            var result = await _consultationService.UpdateConsultationStatusAsync(id, statusDto.Status);
            if (!result)
                return NotFound(new { message = "Consultation not found" });

            return Ok(new { message = "Consultation status updated successfully" });
        }

        /// <summary>
        /// Get consultation statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ConsultationStatsDto), 200)]
        public async Task<ActionResult<ConsultationStatsDto>> GetConsultationStats()
        {
            var stats = await _consultationService.GetConsultationStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get all consultation modes
        /// </summary>
        [HttpGet("modes")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetConsultationModes()
        {
            var modes = await _consultationService.GetConsultationModesAsync();
            return Ok(modes);
        }

        /// <summary>
        /// Check if consultation exists
        /// </summary>
        [HttpGet("exists/{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckConsultationExists(int id)
        {
            var exists = await _consultationService.ConsultationExistsAsync(id);
            return Ok(exists);
        }
    }
}
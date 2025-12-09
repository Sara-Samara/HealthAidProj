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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Get all patients with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(PagedResult<PatientDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<PatientDto>>> GetPatients([FromQuery] PatientFilterDto filter)
        {
            try
            {
                var result = await _patientService.GetPatientsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return BadRequest(new { message = "An error occurred while retrieving patients" });
            }
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(typeof(PatientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            return Ok(patient);
        }

        /// <summary>
        /// Get patient by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Manager,Doctor,Patient")]
        [ProducesResponseType(typeof(PatientDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PatientDto>> GetPatientByUser(int userId)
        {
            var patient = await _patientService.GetPatientByUserIdAsync(userId);
            if (patient == null)
                return NotFound(new { message = "Patient not found for this user" });

            return Ok(patient);
        }

        /// <summary>
        /// Create a new patient profile
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PatientDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto patientDto)
        {
            try
            {
                var patient = await _patientService.CreatePatientAsync(patientDto);
                return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, patient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return BadRequest(new { message = "An error occurred while creating patient" });
            }
        }

        /// <summary>
        /// Update patient profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Patient")]
        [ProducesResponseType(typeof(PatientDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PatientDto>> UpdatePatient(int id, UpdatePatientDto patientDto)
        {
            try
            {
                var patient = await _patientService.UpdatePatientAsync(id, patientDto);
                if (patient == null)
                    return NotFound(new { message = "Patient not found" });

                return Ok(patient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {PatientId}", id);
                return BadRequest(new { message = "An error occurred while updating patient" });
            }
        }

        /// <summary>
        /// Delete patient profile
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (!result)
                    return NotFound(new { message = "Patient not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient {PatientId}", id);
                return BadRequest(new { message = "An error occurred while deleting patient" });
            }
        }

        /// <summary>
        /// Toggle patient active status
        /// </summary>
        [HttpPost("{id}/toggle-status")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleActiveStatus(int id)
        {
            var result = await _patientService.ToggleActiveStatusAsync(id);
            if (!result)
                return NotFound(new { message = "Patient not found" });

            return Ok(new { message = "Active status toggled successfully" });
        }

        /// <summary>
        /// Get all blood types
        /// </summary>
        [HttpGet("blood-types")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetBloodTypes()
        {
            var bloodTypes = await _patientService.GetBloodTypesAsync();
            return Ok(bloodTypes);
        }

        /// <summary>
        /// Get patient statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PatientStatsDto), 200)]
        public async Task<ActionResult<PatientStatsDto>> GetPatientStats()
        {
            var stats = await _patientService.GetPatientStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get patients by NGO
        /// </summary>
        [HttpGet("ngo/{ngoId}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<PatientDto>), 200)]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatientsByNGO(int ngoId)
        {
            var patients = await _patientService.GetPatientsByNGOAsync(ngoId);
            return Ok(patients);
        }

        /// <summary>
        /// Get patients by blood type
        /// </summary>
        [HttpGet("blood-type/{bloodType}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<PatientDto>), 200)]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatientsByBloodType(string bloodType)
        {
            var patients = await _patientService.GetPatientsByBloodTypeAsync(bloodType);
            return Ok(patients);
        }

        /// <summary>
        /// Get patients medical summary
        /// </summary>
        [HttpGet("medical-summary")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(IEnumerable<PatientMedicalSummaryDto>), 200)]
        public async Task<ActionResult<IEnumerable<PatientMedicalSummaryDto>>> GetMedicalSummary()
        {
            var summaries = await _patientService.GetPatientsMedicalSummaryAsync();
            return Ok(summaries);
        }

        /// <summary>
        /// Check if patient exists
        /// </summary>
        [HttpGet("exists/{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckPatientExists(int id)
        {
            var exists = await _patientService.PatientExistsAsync(id);
            return Ok(exists);
        }

        /// <summary>
        /// Get total patients count
        /// </summary>
        [HttpGet("count")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> GetTotalPatientsCount()
        {
            var count = await _patientService.GetTotalPatientsCountAsync();
            return Ok(count);
        }
    }
}
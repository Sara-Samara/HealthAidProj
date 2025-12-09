// Controllers/DoctorsController.cs
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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all doctors with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<DoctorDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<DoctorDto>>> GetDoctors([FromQuery] DoctorFilterDto filter)
        {
            try
            {
                var result = await _doctorService.GetDoctorsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors");
                return BadRequest(new { message = "An error occurred while retrieving doctors" });
            }
        }

        /// <summary>
        /// Get doctor by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DoctorDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctor);
        }

        /// <summary>
        /// Get doctor by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(DoctorDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DoctorDto>> GetDoctorByUser(int userId)
        {
            var doctor = await _doctorService.GetDoctorByUserIdAsync(userId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found for this user" });

            return Ok(doctor);
        }

        /// <summary>
        /// Create a new doctor profile
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(DoctorDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DoctorDto>> CreateDoctor(CreateDoctorDto doctorDto)
        {
            try
            {
                var doctor = await _doctorService.CreateDoctorAsync(doctorDto);
                return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor");
                return BadRequest(new { message = "An error occurred while creating doctor" });
            }
        }

        /// <summary>
        /// Update doctor profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(typeof(DoctorDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<DoctorDto>> UpdateDoctor(int id, UpdateDoctorDto doctorDto)
        {
            try
            {
                var doctor = await _doctorService.UpdateDoctorAsync(id, doctorDto);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                return Ok(doctor);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating doctor {DoctorId}", id);
                return BadRequest(new { message = "An error occurred while updating doctor" });
            }
        }

        /// <summary>
        /// Delete doctor profile
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var result = await _doctorService.DeleteDoctorAsync(id);
            if (!result)
                return NotFound(new { message = "Doctor not found" });

            return NoContent();
        }

        /// <summary>
        /// Toggle doctor availability
        /// </summary>
        [HttpPost("{id}/toggle-availability")]
        [Authorize(Roles = "Admin,Manager,Doctor")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var result = await _doctorService.ToggleAvailabilityAsync(id);
            if (!result)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Availability toggled successfully" });
        }

        /// <summary>
        /// Get all specializations
        /// </summary>
        [HttpGet("specializations")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetSpecializations()
        {
            var specializations = await _doctorService.GetSpecializationsAsync();
            return Ok(specializations);
        }

        /// <summary>
        /// Get doctor statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(DoctorStatsDto), 200)]
        public async Task<ActionResult<DoctorStatsDto>> GetDoctorStats()
        {
            var stats = await _doctorService.GetDoctorStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get available doctors
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<DoctorDto>), 200)]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAvailableDoctors()
        {
            var doctors = await _doctorService.GetAvailableDoctorsAsync();
            return Ok(doctors);
        }

        /// <summary>
        /// Get doctors by specialization
        /// </summary>
        [HttpGet("specialization/{specialization}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<DoctorDto>), 200)]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctorsBySpecialization(string specialization)
        {
            var doctors = await _doctorService.GetDoctorsBySpecializationAsync(specialization);
            return Ok(doctors);
        }

        /// <summary>
        /// Check if license number exists
        /// </summary>
        [HttpGet("check-license/{licenseNumber}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckLicenseNumberExists(string licenseNumber)
        {
            var exists = await _doctorService.LicenseNumberExistsAsync(licenseNumber);
            return Ok(exists);
        }
    }
}
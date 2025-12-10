using HealthAidAPI.DTOs.Emergency; // تأكد من وجود الـ DTOs هنا
using HealthAidAPI.Helpers;
using HealthAidAPI.Models.Emergency; // لاستخدام EmergencyResponder إذا لزم الأمر كـ Return Type مؤقت
using HealthAidAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmergencyController : ControllerBase
    {
        // الاعتماد على Abstraction (Interface) وليس Implementation (Service/DbContext)
        private readonly IEmergencyService _emergencyService;
        private readonly ILogger<EmergencyController> _logger;

        // حقن التبعيات (Dependency Injection)
        public EmergencyController(IEmergencyService emergencyService, ILogger<EmergencyController> logger)
        {
            _emergencyService = emergencyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all emergency cases with optional status filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmergencyCaseDto>>>> GetEmergencyCases([FromQuery] string? status = null)
        {
            try
            {
                var cases = await _emergencyService.GetEmergencyCasesAsync(status);
                return Ok(new ApiResponse<List<EmergencyCaseDto>>
                {
                    Success = true,
                    Message = "Emergency cases retrieved successfully",
                    Data = cases
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency cases");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving emergency cases"
                });
            }
        }

        /// <summary>
        /// Get a specific emergency case by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmergencyCaseDto>>> GetEmergencyCase(int id)
        {
            try
            {
                var emergencyCase = await _emergencyService.GetEmergencyCaseByIdAsync(id);

                if (emergencyCase == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Emergency case with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EmergencyCaseDto>
                {
                    Success = true,
                    Message = "Emergency case retrieved successfully",
                    Data = emergencyCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency case {EmergencyId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the emergency case"
                });
            }
        }

        /// <summary>
        /// Create a new emergency alert
        /// </summary>
        [HttpPost("alert")]
        public async Task<ActionResult<ApiResponse<EmergencyCaseDto>>> CreateEmergencyAlert(CreateEmergencyCaseDto createDto)
        {
            try
            {
                var createdCase = await _emergencyService.CreateEmergencyAlertAsync(createDto);

                return CreatedAtAction(nameof(GetEmergencyCase), new { id = createdCase.Id },
                    new ApiResponse<EmergencyCaseDto>
                    {
                        Success = true,
                        Message = "Emergency alert created successfully",
                        Data = createdCase
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency alert");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the emergency alert"
                });
            }
        }

        /// <summary>
        /// Assign a responder to an emergency case
        /// </summary>
        [HttpPut("{id}/assign-responder")]
        public async Task<ActionResult<ApiResponse<EmergencyCaseDto>>> AssignResponder(int id, AssignResponderDto assignDto)
        {
            try
            {
                var updatedCase = await _emergencyService.AssignResponderAsync(id, assignDto);

                if (updatedCase == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Emergency case with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EmergencyCaseDto>
                {
                    Success = true,
                    Message = "Responder assigned successfully",
                    Data = updatedCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning responder to emergency {EmergencyId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while assigning the responder"
                });
            }
        }

        /// <summary>
        /// Get nearby active responders
        /// </summary>
        [HttpGet("nearby-responders")]
        public async Task<ActionResult<ApiResponse<List<EmergencyResponder>>>> GetNearbyResponders(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 10.00m)
        {
            try
            {
                var responders = await _emergencyService.GetNearbyRespondersAsync(latitude, longitude, radius);

                return Ok(new ApiResponse<List<EmergencyResponder>>
                {
                    Success = true,
                    Message = "Nearby responders retrieved successfully",
                    Data = responders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby responders");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving nearby responders"
                });
            }
        }

        /// <summary>
        /// Register a new emergency responder
        /// </summary>
        [HttpPost("responders")]
        public async Task<ActionResult<ApiResponse<EmergencyResponder>>> CreateEmergencyResponder(EmergencyResponder responder)
        {
            // ملاحظة: يفضل مستقبلاً استخدام CreateResponderDto هنا بدلاً من Entity مباشرة
            try
            {
                var createdResponder = await _emergencyService.CreateResponderAsync(responder);

                return CreatedAtAction(nameof(GetNearbyResponders),
                    new { latitude = createdResponder.Latitude, longitude = createdResponder.Longitude },
                    new ApiResponse<EmergencyResponder>
                    {
                        Success = true,
                        Message = "Emergency responder created successfully",
                        Data = createdResponder
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency responder");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the emergency responder"
                });
            }
        }
    }
}

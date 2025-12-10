using HealthAidAPI.DTOs.Recommendations;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(IRecommendationService recommendationService, ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        // GET: api/recommendation/doctors/5
        // يولد توصيات جديدة ويعرضها
        [HttpGet("doctors/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<DoctorRecommendationDto>>>> GenerateDoctorRecommendations(int patientId)
        {
            try
            {
                var recommendations = await _recommendationService.GenerateDoctorRecommendationsAsync(patientId);
                return Ok(new ApiResponse<List<DoctorRecommendationDto>>
                {
                    Success = true,
                    Message = "Doctor recommendations generated successfully",
                    Data = recommendations
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        // GET: api/recommendation/recommendations/5
        // يعرض التوصيات المحفوظة (غير المشاهدة)
        [HttpGet("recommendations/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<DoctorRecommendationDto>>>> GetStoredRecommendations(int patientId)
        {
            try
            {
                var recommendations = await _recommendationService.GetStoredRecommendationsAsync(patientId);
                return Ok(new ApiResponse<List<DoctorRecommendationDto>>
                {
                    Success = true,
                    Message = "Patient recommendations retrieved successfully",
                    Data = recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stored recommendations for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        // POST: api/recommendation/health-profile
        [HttpPost("health-profile")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfileDto>>> CreateHealthProfile(CreateHealthProfileDto request)
        {
            try
            {
                var profile = await _recommendationService.CreateHealthProfileAsync(request);
                return CreatedAtAction(nameof(GetHealthProfile), new { patientId = request.PatientId },
                    new ApiResponse<PatientHealthProfileDto>
                    {
                        Success = true,
                        Message = "Health profile created successfully",
                        Data = profile
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating health profile");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        // GET: api/recommendation/health-profile/5
        [HttpGet("health-profile/{patientId}")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfileDto>>> GetHealthProfile(int patientId)
        {
            try
            {
                var profile = await _recommendationService.GetHealthProfileAsync(patientId);

                if (profile == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Health profile not found"
                    });
                }

                return Ok(new ApiResponse<PatientHealthProfileDto>
                {
                    Success = true,
                    Message = "Health profile retrieved successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health profile for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        // PUT: api/recommendation/health-profile/5
        [HttpPut("health-profile/{patientId}")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfileDto>>> UpdateHealthProfile(int patientId, UpdateHealthProfileDto request)
        {
            try
            {
                var profile = await _recommendationService.UpdateHealthProfileAsync(patientId, request);

                if (profile == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Health profile not found"
                    });
                }

                return Ok(new ApiResponse<PatientHealthProfileDto>
                {
                    Success = true,
                    Message = "Health profile updated successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating health profile for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }
    }
}
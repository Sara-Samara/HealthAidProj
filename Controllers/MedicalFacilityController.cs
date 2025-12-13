using HealthAidAPI.DTOs.MedicalFacilities;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicalFacilityController : ControllerBase
    {
        private readonly IMedicalFacilityService _medicalFacilityService;
        private readonly ILogger<MedicalFacilityController> _logger;

        public MedicalFacilityController(IMedicalFacilityService medicalFacilityService, ILogger<MedicalFacilityController> logger)
        {
            _medicalFacilityService = medicalFacilityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MedicalFacilityDto>>>> GetMedicalFacilities(
            [FromQuery] string? type = null,
            [FromQuery] bool? verified = null,
            [FromQuery] decimal? minRating = null)
        {
            try
            {
                var facilities = await _medicalFacilityService.GetMedicalFacilitiesAsync(type, verified, minRating);
                return Ok(new ApiResponse<List<MedicalFacilityDto>>
                {
                    Success = true,
                    Message = "Medical facilities retrieved successfully",
                    Data = facilities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical facilities");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalFacilityDto>>> GetMedicalFacility(int id)
        {
            try
            {
                var facility = await _medicalFacilityService.GetMedicalFacilityByIdAsync(id);

                if (facility == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Medical facility not found"
                    });
                }

                return Ok(new ApiResponse<MedicalFacilityDto>
                {
                    Success = true,
                    Message = "Medical facility retrieved successfully",
                    Data = facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MedicalFacilityDto>>> CreateMedicalFacility(CreateMedicalFacilityDto request)
        {
            try
            {
                var facility = await _medicalFacilityService.CreateMedicalFacilityAsync(request);

                return CreatedAtAction(nameof(GetMedicalFacility), new { id = facility.Id },
                    new ApiResponse<MedicalFacilityDto>
                    {
                        Success = true,
                        Message = "Medical facility created successfully",
                        Data = facility
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical facility");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalFacilityDto>>> UpdateMedicalFacility(int id, UpdateMedicalFacilityDto request)
        {
            try
            {
                var facility = await _medicalFacilityService.UpdateMedicalFacilityAsync(id, request);
                if (facility == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Medical facility not found"
                    });
                }

                return Ok(new ApiResponse<MedicalFacilityDto>
                {
                    Success = true,
                    Message = "Medical facility updated successfully",
                    Data = facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<ApiResponse<FacilityReviewDto>>> AddFacilityReview(int id, CreateFacilityReviewDto request)
        {
            try
            {
                var review = await _medicalFacilityService.AddFacilityReviewAsync(id, request);

                return CreatedAtAction(nameof(GetMedicalFacility), new { id },
                    new ApiResponse<FacilityReviewDto>
                    {
                        Success = true,
                        Message = "Review added successfully",
                        Data = review
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review to facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<ApiResponse<List<MedicalFacilityDto>>>> GetNearbyFacilities(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5.00m,
            [FromQuery] string? type = null)
        {
            try
            {
                var facilities = await _medicalFacilityService.GetNearbyFacilitiesAsync(latitude, longitude, radius, type);

                return Ok(new ApiResponse<List<MedicalFacilityDto>>
                {
                    Success = true,
                    Message = "Nearby medical facilities retrieved successfully",
                    Data = facilities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby facilities");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }
}
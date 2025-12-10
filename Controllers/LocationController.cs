using HealthAidAPI.DTOs.Locations;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpPost("update")]
        public async Task<ActionResult<ApiResponse<UserLocationDto>>> UpdateUserLocation(UpdateUserLocationDto request)
        {
            try
            {
                var location = await _locationService.UpdateUserLocationAsync(request);
                return Ok(new ApiResponse<UserLocationDto>
                {
                    Success = true,
                    Message = "Location updated successfully",
                    Data = location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user location");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<UserLocationDto>>>> GetUserLocations(int userId)
        {
            try
            {
                var locations = await _locationService.GetUserLocationsAsync(userId);
                return Ok(new ApiResponse<List<UserLocationDto>>
                {
                    Success = true,
                    Message = "User locations retrieved successfully",
                    Data = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user locations");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("emergency-services")]
        public async Task<ActionResult<ApiResponse<EmergencyServicesResponseDto>>> GetEmergencyServices(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5.00m)
        {
            try
            {
                var response = await _locationService.GetEmergencyServicesAsync(latitude, longitude, radius);
                return Ok(new ApiResponse<EmergencyServicesResponseDto>
                {
                    Success = true,
                    Message = "Emergency services retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency services");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("service-areas")]
        public async Task<ActionResult<ApiResponse<List<ServiceAreaDto>>>> GetServiceAreas()
        {
            try
            {
                var areas = await _locationService.GetServiceAreasAsync();
                return Ok(new ApiResponse<List<ServiceAreaDto>>
                {
                    Success = true,
                    Message = "Service areas retrieved successfully",
                    Data = areas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service areas");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("service-areas")]
        public async Task<ActionResult<ApiResponse<ServiceAreaDto>>> CreateServiceArea(CreateServiceAreaDto request)
        {
            try
            {
                var area = await _locationService.CreateServiceAreaAsync(request);
                return CreatedAtAction(nameof(GetServiceAreas),
                    new ApiResponse<ServiceAreaDto>
                    {
                        Success = true,
                        Message = "Service area created successfully",
                        Data = area
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service area");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }
}
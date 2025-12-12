using HealthAidAPI.DTOs.Locations;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(
            ILocationService locationService,
            ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        // ============================
        // 🔐 Auth Helper (موحّد)
        // ============================
        private int GetCurrentUserId()
        {
            var claim =
                User.FindFirst("id") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("nameid") ??
                User.FindFirst("sub");

            if (claim == null)
                throw new UnauthorizedAccessException("User not logged in");

            if (!int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid user id in token");

            return userId;
        }

        private IActionResult UnauthorizedUser() =>
            Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "You must be logged in to perform this action."
            });

        // =========================================================
        // 📌 Update User Location (Current User Only)
        // =========================================================
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserLocation(UpdateUserLocationDto request)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var internalRequest = new UpdateUserLocationDto
                {
                    UserId = currentUserId, // 👈 من التوكن فقط
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address,
                    City = request.City,
                    Region = request.Region,
                    Accuracy = request.Accuracy,
                    LocationType = request.LocationType,
                    IsPrimary = request.IsPrimary
                };

                var location = await _locationService.UpdateUserLocationAsync(internalRequest);

                return Ok(new ApiResponse<UserLocationDto>
                {
                    Success = true,
                    Message = "Location updated successfully",
                    Data = location
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
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

        // =========================================================
        // 📌 Get Locations for Current User
        // =========================================================
        [HttpGet("user")]
        public async Task<IActionResult> GetUserLocations()
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var locations = await _locationService.GetUserLocationsAsync(currentUserId);

                return Ok(new ApiResponse<List<UserLocationDto>>
                {
                    Success = true,
                    Message = "User locations retrieved successfully",
                    Data = locations
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
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

        // =========================================================
        // 📌 Get Emergency Services Near Location
        // =========================================================
        [HttpGet("emergency-services")]
        public async Task<IActionResult> GetEmergencyServices(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5.00m)
        {
            try
            {
                GetCurrentUserId(); // فقط تحقق تسجيل الدخول

                var response =
                    await _locationService.GetEmergencyServicesAsync(latitude, longitude, radius);

                return Ok(new ApiResponse<EmergencyServicesResponseDto>
                {
                    Success = true,
                    Message = "Emergency services retrieved successfully",
                    Data = response
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
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

        // =========================================================
        // 📌 Get Service Areas (Any Logged User)
        // =========================================================
        [HttpGet("service-areas")]
        public async Task<IActionResult> GetServiceAreas()
        {
            try
            {
                GetCurrentUserId(); // تحقق تسجيل الدخول فقط

                var areas = await _locationService.GetServiceAreasAsync();

                return Ok(new ApiResponse<List<ServiceAreaDto>>
                {
                    Success = true,
                    Message = "Service areas retrieved successfully",
                    Data = areas
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
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

        // =========================================================
        // 📌 Create Service Area (Admin / Auth only)
        // =========================================================
        [HttpPost("service-areas")]
        public async Task<IActionResult> CreateServiceArea(CreateServiceAreaDto request)
        {
            try
            {
                GetCurrentUserId(); 

                var area = await _locationService.CreateServiceAreaAsync(request);

                return Ok(new ApiResponse<ServiceAreaDto>
                {
                    Success = true,
                    Message = "Service area created successfully",
                    Data = area
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
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

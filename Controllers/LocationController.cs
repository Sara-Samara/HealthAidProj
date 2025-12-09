using HealthAidAPI.Data;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Emergency;
using HealthAidAPI.Models.Location;
using HealthAidAPI.Models.MedicalFacilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ApplicationDbContext context, ILogger<LocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/location/update
        [HttpPost("update")]
        public async Task<ActionResult<ApiResponse<UserLocation>>> UpdateUserLocation(UpdateLocationRequest request)
        {
            try
            {
                var location = new UserLocation
                {
                    UserId = request.UserId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address,
                    City = request.City,
                    Region = request.Region,
                    Accuracy = request.Accuracy,
                    LocationType = request.LocationType,
                    IsPrimary = request.IsPrimary,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserLocations.Add(location);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<UserLocation>
                {
                    Success = true,
                    Message = "Location updated successfully",
                    Data = location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user location");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/location/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<UserLocation>>>> GetUserLocations(int userId)
        {
            try
            {
                var locations = await _context.UserLocations
                    .Where(ul => ul.UserId == userId)
                    .OrderByDescending(ul => ul.CreatedAt)
                    .ToListAsync();

                return Ok(new ApiResponse<List<UserLocation>>
                {
                    Success = true,
                    Message = "User locations retrieved successfully",
                    Data = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user locations for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/location/emergency-services
        [HttpGet("emergency-services")]
        public async Task<ActionResult<ApiResponse<EmergencyServicesResponse>>> GetEmergencyServices(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5.00m)
        {
            try
            {
                var hospitals = await _context.MedicalFacilities
                    .Where(f => f.Type == "Hospital" && f.IsActive && f.Verified)
                    .Take(10)
                    .ToListAsync();

                var responders = await _context.EmergencyResponders
                    .Where(r => r.IsAvailable)
                    .Include(r => r.User)
                    .Take(10)
                    .ToListAsync();

                var response = new EmergencyServicesResponse
                {
                    Hospitals = hospitals,
                    EmergencyResponders = responders
                };

                return Ok(new ApiResponse<EmergencyServicesResponse>
                {
                    Success = true,
                    Message = "Emergency services retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency services");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/location/service-areas
        [HttpGet("service-areas")]
        public async Task<ActionResult<ApiResponse<List<ServiceArea>>>> GetServiceAreas()
        {
            try
            {
                var areas = await _context.ServiceAreas
                    .Where(sa => sa.IsActive)
                    .ToListAsync();

                return Ok(new ApiResponse<List<ServiceArea>>
                {
                    Success = true,
                    Message = "Service areas retrieved successfully",
                    Data = areas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service areas");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/location/service-areas
        [HttpPost("service-areas")]
        public async Task<ActionResult<ApiResponse<ServiceArea>>> CreateServiceArea(ServiceAreaRequest request)
        {
            try
            {
                var area = new ServiceArea
                {
                    AreaName = request.AreaName,
                    City = request.City,
                    Region = request.Region,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Radius = request.Radius,
                    Description = request.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ServiceAreas.Add(area);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServiceAreas),
                    new ApiResponse<ServiceArea>
                    {
                        Success = true,
                        Message = "Service area created successfully",
                        Data = area
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service area");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }

    public class UpdateLocationRequest
    {
        public int UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Accuracy { get; set; }
        public string LocationType { get; set; } = "Current";
        public bool IsPrimary { get; set; } = false;
    }

    public class EmergencyServicesResponse
    {
        public List<MedicalFacility> Hospitals { get; set; } = new();
        public List<EmergencyResponder> EmergencyResponders { get; set; } = new();
    }

    public class ServiceAreaRequest
    {
        public required string AreaName { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal Radius { get; set; } = 10.00m;
        public string Description { get; set; } = string.Empty;
    }
}
using HealthAidAPI.Data;
using HealthAidAPI.Models;
using HealthAidAPI.Models.MedicalFacilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalFacilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalFacilityController> _logger;

        public MedicalFacilityController(ApplicationDbContext context, ILogger<MedicalFacilityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/medicalfacility
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MedicalFacility>>>> GetMedicalFacilities(
            [FromQuery] string? type = null,
            [FromQuery] bool? verified = null,
            [FromQuery] decimal? minRating = null)
        {
            try
            {
                var query = _context.MedicalFacilities.AsQueryable();

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(f => f.Type == type);

                if (verified.HasValue)
                    query = query.Where(f => f.Verified == verified.Value);

                if (minRating.HasValue)
                    query = query.Where(f => f.AverageRating >= minRating.Value);

                var facilities = await query
                    .OrderByDescending(f => f.AverageRating)
                    .ThenBy(f => f.Name)
                    .ToListAsync();

                return Ok(new ApiResponse<List<MedicalFacility>>
                {
                    Success = true,
                    Message = "Medical facilities retrieved successfully",
                    Data = facilities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical facilities");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/medicalfacility/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalFacility>>> GetMedicalFacility(int id)
        {
            try
            {
                var facility = await _context.MedicalFacilities
                    .Include(f => f.Reviews)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (facility == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Medical facility not found"
                    });

                return Ok(new ApiResponse<MedicalFacility>
                {
                    Success = true,
                    Message = "Medical facility retrieved successfully",
                    Data = facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/medicalfacility
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MedicalFacility>>> CreateMedicalFacility(MedicalFacilityRequest request)
        {
            try
            {
                var facility = new MedicalFacility
                {
                    Name = request.Name,
                    Type = request.Type,
                    Address = request.Address,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    ContactNumber = request.ContactNumber,
                    Email = request.Email,
                    Services = request.Services,
                    OperatingHours = request.OperatingHours,
                    IsActive = true,
                    Verified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalFacilities.Add(facility);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMedicalFacility), new { id = facility.Id },
                    new ApiResponse<MedicalFacility>
                    {
                        Success = true,
                        Message = "Medical facility created successfully",
                        Data = facility
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical facility");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // PUT: api/medicalfacility/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MedicalFacility>>> UpdateMedicalFacility(int id, MedicalFacilityRequest request)
        {
            try
            {
                var facility = await _context.MedicalFacilities.FindAsync(id);
                if (facility == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Medical facility not found"
                    });

                facility.Name = request.Name;
                facility.Type = request.Type;
                facility.Address = request.Address;
                facility.Latitude = request.Latitude;
                facility.Longitude = request.Longitude;
                facility.ContactNumber = request.ContactNumber;
                facility.Email = request.Email;
                facility.Services = request.Services;
                facility.OperatingHours = request.OperatingHours;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<MedicalFacility>
                {
                    Success = true,
                    Message = "Medical facility updated successfully",
                    Data = facility
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medical facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/medicalfacility/5/reviews
        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<ApiResponse<FacilityReview>>> AddFacilityReview(int id, FacilityReviewRequest request)
        {
            try
            {
                var review = new FacilityReview
                {
                    FacilityId = id,
                    UserId = request.UserId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FacilityReviews.Add(review);

                // Update facility rating
                await UpdateFacilityRating(id);

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMedicalFacility), new { id },
                    new ApiResponse<FacilityReview>
                    {
                        Success = true,
                        Message = "Review added successfully",
                        Data = review
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review to facility {FacilityId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/medicalfacility/nearby
        [HttpGet("nearby")]
        public async Task<ActionResult<ApiResponse<List<MedicalFacility>>>> GetNearbyFacilities(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 5.00m,
            [FromQuery] string? type = null)
        {
            try
            {
                var facilities = await _context.MedicalFacilities
                    .Where(f => f.IsActive && f.Verified)
                    .ToListAsync();

                var nearbyFacilities = facilities
                    .Where(f => f.Latitude.HasValue && f.Longitude.HasValue)
                    .Where(f => CalculateDistance(latitude, longitude, f.Latitude.Value, f.Longitude.Value) <= radius)
                    .ToList();

                if (!string.IsNullOrEmpty(type))
                    nearbyFacilities = nearbyFacilities.Where(f => f.Type == type).ToList();

                return Ok(new ApiResponse<List<MedicalFacility>>
                {
                    Success = true,
                    Message = "Nearby medical facilities retrieved successfully",
                    Data = nearbyFacilities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby facilities");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        private async Task UpdateFacilityRating(int facilityId)
        {
            var reviews = await _context.FacilityReviews
                .Where(r => r.FacilityId == facilityId)
                .ToListAsync();

            if (reviews.Any())
            {
                var averageRating = reviews.Average(r => r.Rating);
                var totalReviews = reviews.Count;

                var facility = await _context.MedicalFacilities.FindAsync(facilityId);
                if (facility != null)
                {
                    facility.AverageRating = (decimal)averageRating;
                    facility.TotalReviews = totalReviews;
                }
            }
        }

        private static decimal CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            return Math.Abs(lat1 - lat2) + Math.Abs(lon1 - lon2);
        }
    }

    public class MedicalFacilityRequest
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Services { get; set; } = string.Empty;
        public string OperatingHours { get; set; } = string.Empty;
    }

    public class FacilityReviewRequest
    {
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
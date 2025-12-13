using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            IRatingService ratingService,
            ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        // ============================
        // 🔐 Auth Helpers (مثل ما طلبتي)
        // ============================
        private int GetCurrentUserId()
        {
            var claim =
                User.FindFirst("id") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("nameid");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("User not logged in");

            return userId;
        }

        private bool IsAdmin =>
            User.IsInRole("Admin") ||
            User.IsInRole("Manager") ||
            User.IsInRole("Finance");

        private IActionResult UnauthorizedResponse(string msg = "You must be logged in.")
            => Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = msg
            });

      
        [HttpGet]
        
        public async Task<IActionResult> GetRatings([FromQuery] RatingFilterDto filter)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

          
                if (!IsAdmin)
                    filter.UserId = currentUserId;

                var result = await _ratingService.GetAllRatingsAsync(filter);

                return Ok(new ApiResponse<PagedResult<RatingDto>>
                {
                    Success = true,
                    Message = "Ratings retrieved successfully",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return UnauthorizedResponse(ex.Message);
            }
        }

      
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRating(int id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);

            if (rating == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rating not found"
                });
            }

            return Ok(new ApiResponse<RatingDto>
            {
                Success = true,
                Message = "Rating retrieved successfully",
                Data = rating
            });
        }


        [HttpPost]
        
        public async Task<IActionResult> CreateRating(CreateRatingDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                dto.UserId = currentUserId;

                var rating = await _ratingService.CreateRatingAsync(dto);

                return CreatedAtAction(nameof(GetRating),
                    new { id = rating.RatingId },
                    new ApiResponse<RatingDto>
                    {
                        Success = true,
                        Message = "Rating created successfully",
                        Data = rating
                    });
            }
            catch (UnauthorizedAccessException ex)
            {
                return UnauthorizedResponse();
            }
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRating(int id, UpdateRatingDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var updated = await _ratingService.UpdateRatingAsync(id, dto, currentUserId);

                if (updated == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Rating not found or insufficient permissions"
                    });
                }

                return Ok(new ApiResponse<RatingDto>
                {
                    Success = true,
                    Message = "Rating updated successfully",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return UnauthorizedResponse(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRating(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var deleted = await _ratingService.DeleteRatingAsync(
                    id,
                    currentUserId,
                    IsAdmin
                );

                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Rating not found or insufficient permissions"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Rating deleted successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return UnauthorizedResponse(ex.Message);
            }
        }

        // ============================
        // GET Ratings by Target (Public)
        // ============================
        [HttpGet("target/{targetType}/{targetId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingsByTarget(string targetType, int targetId)
        {
            var ratings = await _ratingService.GetRatingsByTargetAsync(targetType, targetId);

            return Ok(new ApiResponse<IEnumerable<RatingDto>>
            {
                Success = true,
                Message = "Ratings loaded successfully",
                Data = ratings
            });
        }

        // ============================
        // GET Average Rating (Public)
        // ============================
        [HttpGet("average/{targetType}/{targetId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageRating(string targetType, int targetId)
        {
            var avg = await _ratingService.GetAverageRatingAsync(targetType, targetId);

            return Ok(new ApiResponse<AverageRatingDto>
            {
                Success = true,
                Message = "Average rating calculated successfully",
                Data = avg
            });
        }

        // ============================
        // GET Recent Ratings (Public)
        // ============================
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentRatings([FromQuery] int count = 10)
        {
            var ratings = await _ratingService.GetRecentRatingsAsync(count);

            return Ok(new ApiResponse<IEnumerable<RatingDto>>
            {
                Success = true,
                Message = "Recent ratings retrieved",
                Data = ratings
            });
        }
    }
}

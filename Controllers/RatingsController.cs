// Controllers/RatingsController.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.Helpers;
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
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all ratings with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<RatingDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<RatingDto>>> GetRatings([FromQuery] RatingFilterDto filter)
        {
            try
            {
                var result = await _ratingService.GetAllRatingsAsync(filter);
                return Ok(new ApiResponse<PagedResult<RatingDto>>
                {
                    Success = true,
                    Message = "Ratings retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving ratings"
                });
            }
        }

        /// <summary>
        /// Get rating by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RatingDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RatingDto>> GetRating(int id)
        {
            try
            {
                var rating = await _ratingService.GetRatingByIdAsync(id);
                if (rating == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Rating with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RatingDto>
                {
                    Success = true,
                    Message = "Rating retrieved successfully",
                    Data = rating
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rating {RatingId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the rating"
                });
            }
        }

        /// <summary>
        /// Create a new rating
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RatingDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RatingDto>> CreateRating(CreateRatingDto createRatingDto)
        {
            try
            {
                var rating = await _ratingService.CreateRatingAsync(createRatingDto);
                return CreatedAtAction(nameof(GetRating), new { id = rating.RatingId },
                    new ApiResponse<RatingDto>
                    {
                        Success = true,
                        Message = "Rating created successfully",
                        Data = rating
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the rating"
                });
            }
        }

        /// <summary>
        /// Update a rating
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RatingDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<RatingDto>> UpdateRating(int id, UpdateRatingDto updateRatingDto)
        {
            try
            {
                var rating = await _ratingService.UpdateRatingAsync(id, updateRatingDto);
                if (rating == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Rating with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<RatingDto>
                {
                    Success = true,
                    Message = "Rating updated successfully",
                    Data = rating
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating {RatingId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the rating"
                });
            }
        }

        /// <summary>
        /// Delete a rating
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRating(int id)
        {
            try
            {
                var result = await _ratingService.DeleteRatingAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Rating with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Rating deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating {RatingId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the rating"
                });
            }
        }

        /// <summary>
        /// Get ratings by target
        /// </summary>
        [HttpGet("target/{targetType}/{targetId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RatingDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetRatingsByTarget(string targetType, int targetId)
        {
            try
            {
                var ratings = await _ratingService.GetRatingsByTargetAsync(targetType, targetId);
                if (!ratings.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No ratings found for {targetType} with ID {targetId}"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<RatingDto>>
                {
                    Success = true,
                    Message = $"Ratings for {targetType} {targetId} retrieved successfully",
                    Data = ratings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ratings for {TargetType} {TargetId}", targetType, targetId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving ratings"
                });
            }
        }

        /// <summary>
        /// Get average rating for a target
        /// </summary>
        [HttpGet("average/{targetType}/{targetId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AverageRatingDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AverageRatingDto>> GetAverageRating(string targetType, int targetId)
        {
            try
            {
                var averageRating = await _ratingService.GetAverageRatingAsync(targetType, targetId);
                return Ok(new ApiResponse<AverageRatingDto>
                {
                    Success = true,
                    Message = $"Average rating for {targetType} {targetId} calculated successfully",
                    Data = averageRating
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for {TargetType} {TargetId}", targetType, targetId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while calculating average rating"
                });
            }
        }

        /// <summary>
        /// Get rating statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RatingStatsDto), 200)]
        public async Task<ActionResult<RatingStatsDto>> GetRatingStats()
        {
            try
            {
                var stats = await _ratingService.GetRatingStatsAsync();
                return Ok(new ApiResponse<RatingStatsDto>
                {
                    Success = true,
                    Message = "Rating statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rating statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving rating statistics"
                });
            }
        }

        /// <summary>
        /// Check if user has rated a target
        /// </summary>
        [HttpGet("has-rated/{userId}/{targetType}/{targetId}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> HasUserRated(int userId, string targetType, int targetId)
        {
            try
            {
                var hasRated = await _ratingService.HasUserRatedAsync(userId, targetType, targetId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"User rating status for {targetType} {targetId}",
                    Data = hasRated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user rating status");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while checking rating status"
                });
            }
        }

        /// <summary>
        /// Get recent ratings
        /// </summary>
        [HttpGet("recent")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RatingDto>), 200)]
        public async Task<ActionResult<IEnumerable<RatingDto>>> GetRecentRatings([FromQuery] int count = 10)
        {
            try
            {
                var ratings = await _ratingService.GetRecentRatingsAsync(count);
                return Ok(new ApiResponse<IEnumerable<RatingDto>>
                {
                    Success = true,
                    Message = $"Recent {count} ratings retrieved successfully",
                    Data = ratings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent ratings");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving recent ratings"
                });
            }
        }

        /// <summary>
        /// Delete all ratings by user
        /// </summary>
        [HttpDelete("user/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteRatingsByUser(int userId)
        {
            try
            {
                var result = await _ratingService.DeleteRatingsByUserAsync(userId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No ratings found for user ID {userId}"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"All ratings by user {userId} deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ratings for user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting user ratings"
                });
            }
        }
    }
}
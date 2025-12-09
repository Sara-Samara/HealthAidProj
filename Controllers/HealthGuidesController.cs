// Controllers/HealthGuidesController.cs
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
    [Produces("application/json")]
    public class HealthGuidesController : ControllerBase
    {
        private readonly IHealthGuideService _healthGuideService;
        private readonly ILogger<HealthGuidesController> _logger;

        public HealthGuidesController(IHealthGuideService healthGuideService, ILogger<HealthGuidesController> logger)
        {
            _healthGuideService = healthGuideService;
            _logger = logger;
        }

        /// <summary>
        /// Get all health guides with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<HealthGuideDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<HealthGuideDto>>> GetHealthGuides([FromQuery] HealthGuideFilterDto filter)
        {
            try
            {
                var result = await _healthGuideService.GetHealthGuidesAsync(filter);
                return Ok(new ApiResponse<PagedResult<HealthGuideDto>>
                {
                    Success = true,
                    Message = "Health guides retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health guides");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving health guides"
                });
            }
        }

        /// <summary>
        /// Get health guide by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(HealthGuideDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<HealthGuideDto>> GetHealthGuide(int id)
        {
            try
            {
                var healthGuide = await _healthGuideService.GetHealthGuideByIdAsync(id);
                if (healthGuide == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Health guide with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<HealthGuideDto>
                {
                    Success = true,
                    Message = "Health guide retrieved successfully",
                    Data = healthGuide
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the health guide"
                });
            }
        }

        /// <summary>
        /// Create a new health guide
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,ContentCreator")]
        [ProducesResponseType(typeof(HealthGuideDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<HealthGuideDto>> CreateHealthGuide(CreateHealthGuideDto healthGuideDto)
        {
            try
            {
                var healthGuide = await _healthGuideService.CreateHealthGuideAsync(healthGuideDto);
                return CreatedAtAction(nameof(GetHealthGuide), new { id = healthGuide.HealthGuideId },
                    new ApiResponse<HealthGuideDto>
                    {
                        Success = true,
                        Message = "Health guide created successfully",
                        Data = healthGuide
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating health guide");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the health guide"
                });
            }
        }

        /// <summary>
        /// Update health guide
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor,ContentCreator")]
        [ProducesResponseType(typeof(HealthGuideDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<HealthGuideDto>> UpdateHealthGuide(int id, UpdateHealthGuideDto healthGuideDto)
        {
            try
            {
                var healthGuide = await _healthGuideService.UpdateHealthGuideAsync(id, healthGuideDto);
                if (healthGuide == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Health guide with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<HealthGuideDto>
                {
                    Success = true,
                    Message = "Health guide updated successfully",
                    Data = healthGuide
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating health guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the health guide"
                });
            }
        }

        /// <summary>
        /// Delete health guide
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteHealthGuide(int id)
        {
            try
            {
                var result = await _healthGuideService.DeleteHealthGuideAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Health guide with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Health guide deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting health guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the health guide"
                });
            }
        }

        /// <summary>
        /// Increment like count for a health guide
        /// </summary>
        [HttpPost("{id}/like")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> LikeHealthGuide(int id)
        {
            try
            {
                var result = await _healthGuideService.IncrementLikeCountAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Health guide with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Health guide liked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking health guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while liking the health guide"
                });
            }
        }

        /// <summary>
        /// Toggle publish status of a health guide
        /// </summary>
        [HttpPatch("{id}/toggle-publish")]
        [Authorize(Roles = "Admin,ContentCreator")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> TogglePublishStatus(int id)
        {
            try
            {
                var result = await _healthGuideService.TogglePublishStatusAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Health guide with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Health guide publish status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling publish status for health guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while toggling publish status"
                });
            }
        }

        /// <summary>
        /// Get all available categories
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _healthGuideService.GetCategoriesAsync();
                return Ok(new ApiResponse<IEnumerable<string>>
                {
                    Success = true,
                    Message = "Categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving categories"
                });
            }
        }

        /// <summary>
        /// Get all available languages
        /// </summary>
        [HttpGet("languages")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> GetLanguages()
        {
            try
            {
                var languages = await _healthGuideService.GetLanguagesAsync();
                return Ok(new ApiResponse<IEnumerable<string>>
                {
                    Success = true,
                    Message = "Languages retrieved successfully",
                    Data = languages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving languages");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving languages"
                });
            }
        }

        /// <summary>
        /// Get health guide statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,ContentCreator")]
        [ProducesResponseType(typeof(HealthGuideStatsDto), 200)]
        public async Task<ActionResult<HealthGuideStatsDto>> GetHealthGuideStats()
        {
            try
            {
                var stats = await _healthGuideService.GetHealthGuideStatsAsync();
                return Ok(new ApiResponse<HealthGuideStatsDto>
                {
                    Success = true,
                    Message = "Health guide statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health guide statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving health guide statistics"
                });
            }
        }

        /// <summary>
        /// Get popular health guides
        /// </summary>
        [HttpGet("popular")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<HealthGuideDto>), 200)]
        public async Task<ActionResult<IEnumerable<HealthGuideDto>>> GetPopularGuides([FromQuery] int count = 5)
        {
            try
            {
                var guides = await _healthGuideService.GetPopularGuidesAsync(count);
                return Ok(new ApiResponse<IEnumerable<HealthGuideDto>>
                {
                    Success = true,
                    Message = "Popular health guides retrieved successfully",
                    Data = guides
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular health guides");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving popular health guides"
                });
            }
        }

        /// <summary>
        /// Get related health guides
        /// </summary>
        [HttpGet("{id}/related")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<HealthGuideDto>), 200)]
        public async Task<ActionResult<IEnumerable<HealthGuideDto>>> GetRelatedGuides(int id, [FromQuery] int count = 3)
        {
            try
            {
                var guides = await _healthGuideService.GetRelatedGuidesAsync(id, count);
                return Ok(new ApiResponse<IEnumerable<HealthGuideDto>>
                {
                    Success = true,
                    Message = "Related health guides retrieved successfully",
                    Data = guides
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving related health guides for guide {HealthGuideId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving related health guides"
                });
            }
        }
    }
}
// Controllers/NGOsController.cs
using HealthAidAPI.DTOs.NGO;
using HealthAidAPI.Models;
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
    public class NGOsController : ControllerBase
    {
        private readonly INgoService _ngoService;
        private readonly ILogger<NGOsController> _logger;

        public NGOsController(INgoService ngoService, ILogger<NGOsController> logger)
        {
            _ngoService = ngoService;
            _logger = logger;
        }

        /// <summary>
        /// Get all NGOs with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NgoDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<NgoDto>>> GetNGOs([FromQuery] NgoFilterDto filter)
        {
            try
            {
                var result = await _ngoService.GetAllNgosAsync(filter);
                return Ok(new ApiResponse<PagedResult<NgoDto>>
                {
                    Success = true,
                    Message = "NGOs retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGOs");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGOs"
                });
            }
        }

        /// <summary>
        /// Get NGO by ID with detailed information
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NgoDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NgoDetailDto>> GetNGO(int id)
        {
            try
            {
                var ngo = await _ngoService.GetNgoByIdAsync(id);
                if (ngo == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NgoDetailDto>
                {
                    Success = true,
                    Message = "NGO retrieved successfully",
                    Data = ngo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO {NGOId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the NGO"
                });
            }
        }

        /// <summary>
        /// Create a new NGO
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(NgoDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NgoDto>> CreateNGO(CreateNgoDto createNgoDto)
        {
            try
            {
                var ngo = await _ngoService.CreateNgoAsync(createNgoDto);
                return CreatedAtAction(nameof(GetNGO), new { id = ngo.NGOId },
                    new ApiResponse<NgoDto>
                    {
                        Success = true,
                        Message = "NGO created successfully",
                        Data = ngo
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
                _logger.LogError(ex, "Error creating NGO");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the NGO"
                });
            }
        }

        /// <summary>
        /// Update an NGO
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(NgoDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NgoDto>> UpdateNGO(int id, UpdateNgoDto updateNgoDto)
        {
            try
            {
                var ngo = await _ngoService.UpdateNgoAsync(id, updateNgoDto);
                if (ngo == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NgoDto>
                {
                    Success = true,
                    Message = "NGO updated successfully",
                    Data = ngo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating NGO {NGOId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the NGO"
                });
            }
        }

        /// <summary>
        /// Delete an NGO by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNGO(int id)
        {
            try
            {
                var result = await _ngoService.DeleteNgoAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "NGO deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NGO {NGOId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the NGO"
                });
            }
        }

        /// <summary>
        /// Delete an NGO by name
        /// </summary>
        [HttpDelete("by-name/{name}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNGOByName(string name)
        {
            try
            {
                var result = await _ngoService.DeleteNgoByNameAsync(name);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO with name '{name}' not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"NGO '{name}' deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NGO by name {Name}", name);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the NGO"
                });
            }
        }

        /// <summary>
        /// Get NGOs by verification status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<NgoDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NgoDto>>> GetNGOsByStatus(string status)
        {
            try
            {
                var ngos = await _ngoService.GetNgosByStatusAsync(status);
                if (!ngos.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No NGOs found with status '{status}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<NgoDto>>
                {
                    Success = true,
                    Message = $"NGOs with status '{status}' retrieved successfully",
                    Data = ngos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGOs by status {Status}", status);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGOs by status"
                });
            }
        }

        /// <summary>
        /// Search NGOs by keyword
        /// </summary>
        [HttpGet("search/{keyword}")]
        [ProducesResponseType(typeof(IEnumerable<NgoDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NgoDto>>> SearchNGOs(string keyword)
        {
            try
            {
                var ngos = await _ngoService.SearchNgosAsync(keyword);
                if (!ngos.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No NGOs found matching '{keyword}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<NgoDto>>
                {
                    Success = true,
                    Message = $"NGOs matching '{keyword}' retrieved successfully",
                    Data = ngos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching NGOs with keyword {Keyword}", keyword);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching NGOs"
                });
            }
        }

        /// <summary>
        /// Get NGOs by area of work
        /// </summary>
        [HttpGet("area/{area}")]
        [ProducesResponseType(typeof(IEnumerable<NgoDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NgoDto>>> GetNGOsByArea(string area)
        {
            try
            {
                var ngos = await _ngoService.GetNgosByAreaAsync(area);
                if (!ngos.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No NGOs found in area '{area}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<NgoDto>>
                {
                    Success = true,
                    Message = $"NGOs in area '{area}' retrieved successfully",
                    Data = ngos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGOs by area {Area}", area);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGOs by area"
                });
            }
        }

        /// <summary>
        /// Get NGOs with mission counts
        /// </summary>
        [HttpGet("with-mission-count")]
        [ProducesResponseType(typeof(IEnumerable<NgoDto>), 200)]
        public async Task<ActionResult<IEnumerable<NgoDto>>> GetNGOsWithMissionCount()
        {
            try
            {
                var ngos = await _ngoService.GetNgosWithMissionCountAsync();
                return Ok(new ApiResponse<IEnumerable<NgoDto>>
                {
                    Success = true,
                    Message = "NGOs with mission counts retrieved successfully",
                    Data = ngos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGOs with mission counts");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGOs with mission counts"
                });
            }
        }

        /// <summary>
        /// Get NGO statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(NgoStatsDto), 200)]
        public async Task<ActionResult<NgoStatsDto>> GetNGOStats()
        {
            try
            {
                var stats = await _ngoService.GetNgoStatsAsync();
                return Ok(new ApiResponse<NgoStatsDto>
                {
                    Success = true,
                    Message = "NGO statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGO statistics"
                });
            }
        }

        /// <summary>
        /// Verify/Update NGO status
        /// </summary>
        [HttpPatch("{id}/verify")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyNGO(int id, [FromBody] string status)
        {
            try
            {
                var result = await _ngoService.VerifyNgoAsync(id, status);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"NGO status updated to '{status}' successfully"
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
                _logger.LogError(ex, "Error verifying NGO {NGOId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while verifying the NGO"
                });
            }
        }
    }
}
// Controllers/NgoMissionsController.cs
using HealthAidAPI.DTOs.NgoMissions;
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
    public class NgoMissionsController : ControllerBase
    {
        private readonly INgoMissionService _missionService;
        private readonly ILogger<NgoMissionsController> _logger;

        public NgoMissionsController(INgoMissionService missionService, ILogger<NgoMissionsController> logger)
        {
            _missionService = missionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all NGO missions with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NgoMissionDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<NgoMissionDto>>> GetMissions([FromQuery] NgoMissionFilterDto filter)
        {
            try
            {
                var result = await _missionService.GetAllMissionsAsync(filter);
                return Ok(new ApiResponse<PagedResult<NgoMissionDto>>
                {
                    Success = true,
                    Message = "NGO missions retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO missions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving NGO missions"
                });
            }
        }

        /// <summary>
        /// Get NGO mission by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NgoMissionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NgoMissionDto>> GetMission(int id)
        {
            try
            {
                var mission = await _missionService.GetMissionByIdAsync(id);
                if (mission == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO mission with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NgoMissionDto>
                {
                    Success = true,
                    Message = "NGO mission retrieved successfully",
                    Data = mission
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO mission {MissionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the NGO mission"
                });
            }
        }

        /// <summary>
        /// Create a new NGO mission
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(NgoMissionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NgoMissionDto>> CreateMission(CreateNgoMissionDto createMissionDto)
        {
            try
            {
                var mission = await _missionService.CreateMissionAsync(createMissionDto);
                return CreatedAtAction(nameof(GetMission), new { id = mission.NgoMissionId },
                    new ApiResponse<NgoMissionDto>
                    {
                        Success = true,
                        Message = "NGO mission created successfully",
                        Data = mission
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
                _logger.LogError(ex, "Error creating NGO mission");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the NGO mission"
                });
            }
        }

        /// <summary>
        /// Update an NGO mission
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(NgoMissionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NgoMissionDto>> UpdateMission(int id, UpdateNgoMissionDto updateMissionDto)
        {
            try
            {
                var mission = await _missionService.UpdateMissionAsync(id, updateMissionDto);
                if (mission == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO mission with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NgoMissionDto>
                {
                    Success = true,
                    Message = "NGO mission updated successfully",
                    Data = mission
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
                _logger.LogError(ex, "Error updating NGO mission {MissionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the NGO mission"
                });
            }
        }

        /// <summary>
        /// Delete an NGO mission
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMission(int id)
        {
            try
            {
                var result = await _missionService.DeleteMissionAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"NGO mission with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "NGO mission deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting NGO mission {MissionId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the NGO mission"
                });
            }
        }

        /// <summary>
        /// Delete all missions for a specific NGO
        /// </summary>
        [HttpDelete("ngo/{ngoId}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMissionsByNgo(int ngoId)
        {
            try
            {
                var result = await _missionService.DeleteMissionsByNgoAsync(ngoId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No missions found for NGO {ngoId}"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"All missions for NGO {ngoId} deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting missions for NGO {NGOId}", ngoId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting NGO missions"
                });
            }
        }

        /// <summary>
        /// Search missions by location
        /// </summary>
        [HttpGet("search/{location}")]
        [ProducesResponseType(typeof(IEnumerable<NgoMissionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NgoMissionDto>>> SearchMissionsByLocation(string location)
        {
            try
            {
                var missions = await _missionService.SearchMissionsByLocationAsync(location);
                if (!missions.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No missions found in location '{location}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<NgoMissionDto>>
                {
                    Success = true,
                    Message = $"Missions in '{location}' retrieved successfully",
                    Data = missions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching missions by location {Location}", location);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching missions"
                });
            }
        }

        /// <summary>
        /// Get missions within a date range
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(IEnumerable<NgoMissionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<NgoMissionDto>>> GetMissionsByDateRange([FromQuery] DateRangeDto dateRange)
        {
            try
            {
                var missions = await _missionService.GetMissionsByDateRangeAsync(dateRange);
                if (!missions.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No missions found between {dateRange.Start:yyyy-MM-dd} and {dateRange.End:yyyy-MM-dd}"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<NgoMissionDto>>
                {
                    Success = true,
                    Message = $"Missions between {dateRange.Start:yyyy-MM-dd} and {dateRange.End:yyyy-MM-dd} retrieved successfully",
                    Data = missions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving missions by date range");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving missions by date range"
                });
            }
        }

        /// <summary>
        /// Get mission count for a specific NGO
        /// </summary>
        [HttpGet("count/ngo/{ngoId}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> GetMissionCountByNgo(int ngoId)
        {
            try
            {
                var count = await _missionService.GetMissionCountByNgoAsync(ngoId);
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Mission count for NGO {ngoId}",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mission count for NGO {NGOId}", ngoId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while getting mission count"
                });
            }
        }

        /// <summary>
        /// Get NGO mission statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(MissionStatsDto), 200)]
        public async Task<ActionResult<MissionStatsDto>> GetMissionStats()
        {
            try
            {
                var stats = await _missionService.GetMissionStatsAsync();
                return Ok(new ApiResponse<MissionStatsDto>
                {
                    Success = true,
                    Message = "NGO mission statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO mission statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving mission statistics"
                });
            }
        }

        /// <summary>
        /// Get upcoming missions
        /// </summary>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<NgoMissionDto>), 200)]
        public async Task<ActionResult<IEnumerable<NgoMissionDto>>> GetUpcomingMissions([FromQuery] int days = 30)
        {
            try
            {
                var missions = await _missionService.GetUpcomingMissionsAsync(days);
                return Ok(new ApiResponse<IEnumerable<NgoMissionDto>>
                {
                    Success = true,
                    Message = $"Upcoming missions for the next {days} days retrieved successfully",
                    Data = missions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming missions");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving upcoming missions"
                });
            }
        }
    }
}
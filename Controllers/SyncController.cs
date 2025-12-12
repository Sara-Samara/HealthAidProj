using HealthAidAPI.DTOs.Sync;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("push")]
        public async Task<ActionResult<ApiResponse<SyncResultDto>>> PushOfflineQueue([FromBody] List<SyncRequestDto> queueItems)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _syncService.ProcessOfflineQueueAsync(userId, queueItems);

                return Ok(new ApiResponse<SyncResultDto>
                {
                    Success = true,
                    Message = "Sync processing completed",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sync failed: " + ex.Message
                });
            }
        }
    }
}
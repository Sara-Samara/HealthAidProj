using HealthAidAPI.DTOs.Notifications;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
 
    [Produces("application/json")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // ============================
        // 🔐 Auth Helpers
        // ============================
        private int GetCurrentUserId()
        {
            var claim =
                User.FindFirst("id") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("sub");

            if (claim == null)
                throw new UnauthorizedAccessException("User not logged in");

            if (!int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid user id in token");

            return userId;
        }

        private bool IsAdmin =>
            User.IsInRole("Admin") || User.IsInRole("Manager");

        private IActionResult UnauthorizedUser() =>
            Unauthorized(new { 
                sucsess=false,

                Message = "You must be logged in to perform this action." });

        private IActionResult ForbiddenUser() =>
            Forbid();

        // ============================
        // GET Notifications
        // ============================
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] NotificationFilterDto filter)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                
                if (!IsAdmin && filter.SenderId!=currentUserId)
                {
                    filter.ReceiverId = currentUserId;
                    throw new UnauthorizedAccessException();
                }

                var result = await _notificationService.GetNotificationsAsync(filter);

                return Ok(new ApiResponse<PagedResult<NotificationDto>>
                {
                    Success = true,
                    Message = "Notifications retrieved successfully",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
           return     Unauthorized(new
                {
                    sucsess = false,

                    Message = "You must be logged in to perform this action."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return BadRequest(new { Message = "Failed to load notifications." });
            }
        }

    
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                    throw new NullReferenceException();

                

                return Ok(new ApiResponse<NotificationDto>
                {
                    Success = true,
                    Message = "Notification retrieved successfully",
                    Data = notification
                });
            }

            catch (NullReferenceException)
            {
                return NotFound(new { Message = "Notification not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification");
                return BadRequest(new { Message = "Failed to retrieve notification." });
            }
        }

        // ============================
        // CREATE Notification
        // ============================
        [HttpPost]
        public async Task<IActionResult> CreateNotification(CreateNotificationDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                dto.SenderId = currentUserId;

                var notification = await _notificationService.CreateNotificationAsync(dto);

                return CreatedAtAction(nameof(GetNotification),
                    new { id = notification.NotificationId },
                    new ApiResponse<NotificationDto>
                    {
                        Success = true,
                        Message = "Notification created successfully",
                        Data = notification
                    });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return BadRequest(new { Message = "Failed to create notification." });
            }
        }

        // ============================
        // UPDATE Notification
        // ============================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, UpdateNotificationDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var notif = await _notificationService.GetNotificationByIdAsync(id);
                if (notif == null)
                    return NotFound(new { Message = "Notification not found." });

                if (!IsAdmin && notif.SenderId != currentUserId)
                   throw new UnauthorizedAccessException();

                var updated = await _notificationService.UpdateNotificationAsync(id, dto);

                return Ok(new ApiResponse<NotificationDto>
                {
                    Success = true,
                    Message = "Notification updated successfully",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification");
                return BadRequest(new { Message = "Failed to update notification." });
            }
        }

        // ============================
        // MARK AS READ
        // ============================
        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var notif = await _notificationService.GetNotificationByIdAsync(id);
                if (notif == null)
                    return NotFound(new { Message = "Notification not found." });

                if (!IsAdmin && notif.SenderId != currentUserId)
                    throw new UnauthorizedAccessException();

                await _notificationService.MarkAsReadAsync(id);

                return Ok(new { Message = "Notification marked as read." });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification");
                return BadRequest(new { Message = "Failed to mark notification as read." });
            }
        }

        // ============================
        // DELETE ONE
        // ============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var notif = await _notificationService.GetNotificationByIdAsync(id);
                if (notif == null)
                    return NotFound(new { Message = "Notification not found." });

                if (!IsAdmin && notif.SenderId != currentUserId)
                   throw new UnauthorizedAccessException();

                await _notificationService.DeleteNotificationAsync(id);

                return Ok(new { Message = "Notification deleted successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedUser();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return BadRequest(new { Message = "Failed to delete notification." });
            }
        }

        // ============================
        // DELETE ALL FOR RECEIVER (Admin)
        // ============================
        [HttpDelete("receiver/{receiverId}")]
        public async Task<IActionResult> DeleteNotificationsByReceiver(int receiverId)
        {
            try
            {
             

                await _notificationService.DeleteNotificationsByReceiverAsync(receiverId);

                return Ok(new { Message = "All notifications deleted for this user." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notifications");
                return BadRequest(new { Message = "Failed to delete notifications." });
            }
        }
    }
}

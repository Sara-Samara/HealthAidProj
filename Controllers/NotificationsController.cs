// Controllers/NotificationsController.cs
using HealthAidAPI.DTOs.Notifications;
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
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all notifications with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<NotificationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications([FromQuery] NotificationFilterDto filter)
        {
            try
            {
                var result = await _notificationService.GetNotificationsAsync(filter);
                return Ok(new ApiResponse<PagedResult<NotificationDto>>
                {
                    Success = true,
                    Message = "Notifications retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving notifications"
                });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Notification with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NotificationDto>
                {
                    Success = true,
                    Message = "Notification retrieved successfully",
                    Data = notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the notification"
                });
            }
        }

        /// <summary>
        /// Get notifications by receiver ID
        /// </summary>
        [HttpGet("receiver/{receiverId}")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationsByReceiver(int receiverId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByReceiverAsync(receiverId);
                return Ok(new ApiResponse<IEnumerable<NotificationDto>>
                {
                    Success = true,
                    Message = $"Notifications for receiver {receiverId} retrieved successfully",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for receiver {ReceiverId}", receiverId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving notifications"
                });
            }
        }

        /// <summary>
        /// Get unread notifications for a receiver
        /// </summary>
        [HttpGet("receiver/{receiverId}/unread")]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUnreadNotifications(int receiverId)
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync(receiverId);
                return Ok(new ApiResponse<IEnumerable<NotificationDto>>
                {
                    Success = true,
                    Message = $"Unread notifications for receiver {receiverId} retrieved successfully",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications for receiver {ReceiverId}", receiverId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving unread notifications"
                });
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto createNotificationDto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(createNotificationDto);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId },
                    new ApiResponse<NotificationDto>
                    {
                        Success = true,
                        Message = "Notification created successfully",
                        Data = notification
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
                _logger.LogError(ex, "Error creating notification");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the notification"
                });
            }
        }

        /// <summary>
        /// Create a new notification using user names
        /// </summary>
        [HttpPost("by-name")]
        [ProducesResponseType(typeof(NotificationDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NotificationDto>> CreateNotificationByName(CreateNotificationByNameDto createNotificationDto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationByNameAsync(createNotificationDto);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId },
                    new ApiResponse<NotificationDto>
                    {
                        Success = true,
                        Message = "Notification created successfully",
                        Data = notification
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
                _logger.LogError(ex, "Error creating notification by name");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the notification"
                });
            }
        }

        /// <summary>
        /// Update a notification
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(NotificationDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NotificationDto>> UpdateNotification(int id, UpdateNotificationDto updateNotificationDto)
        {
            try
            {
                var notification = await _notificationService.UpdateNotificationAsync(id, updateNotificationDto);
                if (notification == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Notification with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<NotificationDto>
                {
                    Success = true,
                    Message = "Notification updated successfully",
                    Data = notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification {NotificationId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the notification"
                });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPatch("{id}/mark-read")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Notification with ID {id} not found or already read"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Notification marked as read successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while marking the notification as read"
                });
            }
        }

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        [HttpPatch("mark-read-multiple")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarkMultipleAsRead(MarkNotificationsAsReadDto markAsReadDto)
        {
            try
            {
                var result = await _notificationService.MarkMultipleAsReadAsync(markAsReadDto);
                if (!result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No unread notifications found to mark as read"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Notifications marked as read successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking multiple notifications as read");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while marking notifications as read"
                });
            }
        }

        /// <summary>
        /// Mark all notifications as read for a receiver
        /// </summary>
        [HttpPatch("receiver/{receiverId}/mark-all-read")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarkAllAsRead(int receiverId)
        {
            try
            {
                var result = await _notificationService.MarkAllAsReadAsync(receiverId);
                if (!result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No unread notifications found to mark as read"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "All notifications marked as read successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for receiver {ReceiverId}", receiverId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while marking notifications as read"
                });
            }
        }

        /// <summary>
        /// Get notification statistics
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(NotificationStatsDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats([FromQuery] int? receiverId = null)
        {
            try
            {
                var stats = await _notificationService.GetNotificationStatsAsync(receiverId);
                return Ok(new ApiResponse<NotificationStatsDto>
                {
                    Success = true,
                    Message = receiverId.HasValue ?
                        $"Notification statistics for receiver {receiverId}" :
                        "Global notification statistics",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving notification statistics"
                });
            }
        }

        /// <summary>
        /// Get unread notification count for a receiver
        /// </summary>
        [HttpGet("receiver/{receiverId}/unread-count")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> GetUnreadCount(int receiverId)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(receiverId);
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Unread notification count for receiver {receiverId}",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for receiver {ReceiverId}", receiverId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while getting unread count"
                });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Notification with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Notification deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the notification"
                });
            }
        }

        /// <summary>
        /// Delete all notifications for a receiver
        /// </summary>
        [HttpDelete("receiver/{receiverId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNotificationsByReceiver(int receiverId)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationsByReceiverAsync(receiverId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No notifications found for receiver {receiverId}"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"All notifications for receiver {receiverId} deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notifications for receiver {ReceiverId}", receiverId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting notifications"
                });
            }
        }
    }
}
// Controllers/MessagesController.cs
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
    [Authorize]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all messages with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MessageDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<MessageDto>>> GetMessages([FromQuery] MessageFilterDto filter)
        {
            try
            {
                var result = await _messageService.GetAllMessagesAsync(filter);
                return Ok(new ApiResponse<PagedResult<MessageDto>>
                {
                    Success = true,
                    Message = "Messages retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving messages"
                });
            }
        }

        /// <summary>
        /// Get message by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            try
            {
                var message = await _messageService.GetMessageByIdAsync(id);
                if (message == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Message with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message retrieved successfully",
                    Data = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message {MessageId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the message"
                });
            }
        }

        /// <summary>
        /// Create a new message
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            try
            {
                var message = await _messageService.CreateMessageAsync(createMessageDto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId },
                    new ApiResponse<MessageDto>
                    {
                        Success = true,
                        Message = "Message created successfully",
                        Data = message
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
                _logger.LogError(ex, "Error creating message");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the message"
                });
            }
        }

        /// <summary>
        /// Create a new message using user names
        /// </summary>
        [HttpPost("by-name")]
        [ProducesResponseType(typeof(MessageDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MessageDto>> CreateMessageByName(CreateMessageByNameDto createMessageDto)
        {
            try
            {
                var message = await _messageService.CreateMessageByNameAsync(createMessageDto);
                return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId },
                    new ApiResponse<MessageDto>
                    {
                        Success = true,
                        Message = "Message created successfully",
                        Data = message
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
                _logger.LogError(ex, "Error creating message by name");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the message"
                });
            }
        }

        /// <summary>
        /// Get chat history between two users
        /// </summary>
        [HttpGet("chat")]
        [ProducesResponseType(typeof(IEnumerable<MessageDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetChatBetweenUsers([FromQuery] ChatRequestDto chatRequest)
        {
            try
            {
                var messages = await _messageService.GetChatBetweenUsersAsync(chatRequest);
                return Ok(new ApiResponse<IEnumerable<MessageDto>>
                {
                    Success = true,
                    Message = $"Chat between {chatRequest.SenderName} and {chatRequest.ReceiverName} retrieved successfully",
                    Data = messages
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
                _logger.LogError(ex, "Error retrieving chat between {Sender} and {Receiver}",
                    chatRequest.SenderName, chatRequest.ReceiverName);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the chat"
                });
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPatch("mark-read")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> MarkMessagesAsRead(MarkAsReadDto markAsReadDto)
        {
            try
            {
                var result = await _messageService.MarkMessagesAsReadAsync(markAsReadDto);
                if (!result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No unread messages found to mark as read"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Messages from {markAsReadDto.SenderName} to {markAsReadDto.ReceiverName} marked as read"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while marking messages as read"
                });
            }
        }

        /// <summary>
        /// Get unread message count for a user
        /// </summary>
        [HttpGet("unread-count/{receiverName}")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<int>> GetUnreadCount(string receiverName)
        {
            try
            {
                var count = await _messageService.GetUnreadCountAsync(receiverName);
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Unread message count for {receiverName}",
                    Data = count
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
                _logger.LogError(ex, "Error getting unread count for {Receiver}", receiverName);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while getting unread count"
                });
            }
        }

        /// <summary>
        /// Update a message
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MessageDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MessageDto>> UpdateMessage(int id, UpdateMessageDto updateMessageDto)
        {
            try
            {
                var message = await _messageService.UpdateMessageAsync(id, updateMessageDto);
                if (message == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Message with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message updated successfully",
                    Data = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message {MessageId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the message"
                });
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var result = await _messageService.DeleteMessageAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Message with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Message deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the message"
                });
            }
        }

        /// <summary>
        /// Delete all messages (Admin only)
        /// </summary>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteAllMessages()
        {
            try
            {
                var result = await _messageService.DeleteAllMessagesAsync();
                if (!result)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No messages found to delete"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "All messages deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all messages");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting all messages"
                });
            }
        }

        /// <summary>
        /// Delete all messages for a specific user
        /// </summary>
        [HttpDelete("user/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMessagesByUser(int userId)
        {
            try
            {
                var result = await _messageService.DeleteMessagesByUserAsync(userId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No messages found for user {userId}"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"All messages for user {userId} deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting messages for user {UserId}", userId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting user messages"
                });
            }
        }
    }
}
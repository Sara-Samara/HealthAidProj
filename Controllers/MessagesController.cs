using HealthAidAPI.DTOs.Messages;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessageService messageService,
            ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        
        private int GetCurrentUserId()
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? User.FindFirst("id");

                if (claim == null)
                    throw new UnauthorizedAccessException("User ID not found in token");

                if (!int.TryParse(claim.Value, out int userId))
                    throw new UnauthorizedAccessException("Invalid user ID in token");

                return userId;
            }
            catch (UnauthorizedAccessException){
            
                return -1;
            }
        }

        private bool IsAdmin =>
            User.IsInRole("Admin") || User.IsInRole("Manager");


        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] MessageFilterDto filter)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                if (!IsAdmin && filter.SenderId!=currentUserId)
                {

                    filter.ReceiverId = currentUserId;
                    throw new UnauthorizedAccessException();
                }

                var result = await _messageService.GetAllMessagesAsync(filter);

                return Ok(new ApiResponse<PagedResult<MessageDto>>
                {
                    Success = true,
                    Message = "Messages retrieved successfully",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException)
            {

                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not the user for this account"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to load messages"
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            try
            {
                if (GetCurrentUserId() == -1)
                {
                    throw new UnauthorizedAccessException();
                }

                if (GetCurrentUserId() != id)
                {
                    throw new UnauthorizedAccessException();

                }
                var msg = await _messageService.GetMessageByIdAsync(id);

                if (msg == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Message not found"
                    });
                }

                int currentUserId = GetCurrentUserId();

                if (!IsAdmin &&
                    msg.SenderId != currentUserId &&
                    msg.ReceiverId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(new ApiResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message retrieved successfully",
                    Data = msg
                });
            }
        
            catch (UnauthorizedAccessException)
            {


                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or does'nt have this account or doesn't login"
                });
            }
            catch (NullReferenceException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Message not found"
                });
            }
            catch
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found, please login if you loged out"
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageDto dto)
        {
            try
            {
                dto.SenderId = GetCurrentUserId();
                var message = await _messageService.CreateMessageAsync(dto);
                
                

                return CreatedAtAction(nameof(GetMessage), new { id = message.MessageId },
                    new ApiResponse<MessageDto>
                    {
                        Success = true,
                        Message = "Message created successfully",
                        Data = message
                    });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create message"
                });
            }
        }

        [HttpGet("chat")]
        public async Task<IActionResult> GetChatBetweenUsers([FromQuery] ChatRequestDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                if (!IsAdmin && dto.SenderId != currentUserId)
                    throw new UnauthorizedAccessException();

                var messages = await _messageService.GetChatBetweenUsersAsync(dto);

                return Ok(new ApiResponse<IEnumerable<MessageDto>>
                {
                    Success = true,
                    Message = "Chat retrieved successfully",
                    Data = messages
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to load chat"
                });
            }
        }

        [HttpPatch("mark-read")]
      
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkAsReadDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

              
                if (!IsAdmin)
                {
                    dto.MyId = currentUserId; 
                }

                var result = await _messageService.MarkMessagesAsReadAsync(dto);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No unread messages found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Messages marked as read"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied"
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, UpdateMessageDto dto)
        {
            try
            {

                var msg = await _messageService.GetMessageByIdAsync(id);
                if (msg == null)
                    throw new ArgumentNullException();

                int currentUserId = GetCurrentUserId();

                if (!IsAdmin && msg.SenderId != currentUserId)
                    throw new UnauthorizedAccessException();

                var updated = await _messageService.UpdateMessageAsync(id, dto);

                return Ok(new ApiResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message updated successfully",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });
            }
            catch (ArgumentNullException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Message not found"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update message"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {

                var msg = await _messageService.GetMessageByIdAsync(id);
                if (msg == null)
                    throw new NullReferenceException();

                int currentUserId = GetCurrentUserId();

                if (!IsAdmin &&
                    msg.SenderId != currentUserId &&
                    msg.ReceiverId != currentUserId)
                {
                    throw new MissingFieldException();
                }

                await _messageService.DeleteMessageAsync(id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Message deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });

            }
            catch (NullReferenceException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Message not found"
                });
            }
            catch (MissingFieldException)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to delete message"
                });
            }

            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete message"
                });
            }
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAllMessages()
        {
            try
            {

                if (!IsAdmin)
                {
                    throw new UnauthorizedAccessException();
                }

                await _messageService.DeleteAllMessagesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "All messages deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete all messages"
                });
            }
        }


        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteMessagesByUser(int userId)
        {
            try
            {
                if (!IsAdmin)
                {
                    throw new UnauthorizedAccessException();
                }

                await _messageService.DeleteMessagesByUserAsync(userId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User messages deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Access denied you are not admin or not use the same  id that login in this account"
                });
            }
            catch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete user messages"
                });
            }
        }
    }
}

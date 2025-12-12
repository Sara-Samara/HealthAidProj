// Controllers/UsersController.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Auth;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // =========================
        // Helper: Current User ID
        // =========================
        private int GetCurrentUserId()
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
        
        // =====================================================
        // SEARCH / GET ALL USERS
        // Admin / Manager ONLY
        // =====================================================

        [HttpGet]
      
        [ProducesResponseType(typeof(PagedResult<UserDto>), 200)]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
        {

            try
            {
                string role = User.FindFirst(ClaimTypes.Role)?.Value!;
                if (role == "User" || role == "user")
                {
                    throw new UnauthorizedAccessException();
                }
                var result = await _userService.GetUsersAsync(filter);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException) { 
                return Unauthorized(new
                {
                    success = false,
                    message = "Not Allowed to accsess beacuse you are user"
                });
            }
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try {
                int currentUserId = GetCurrentUserId();
                bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");

                if (!isAdmin && currentUserId != id)
                {
                    throw new UnauthorizedAccessException();
                }

                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                    throw new KeyNotFoundException();

                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "You are not allowed to access this user."
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }
        }

        // =====================================================
        // UPDATE USER
        // Admin ➜ any user
        // User  ➜ himself only
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto userDto)
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");

            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "You are not allowed to update this user."
                });
            }

            var user = await _userService.UpdateUserAsync(id, userDto);

            if (user == null)
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });

            return Ok(new
            {
                success = true,
                message = "User updated successfully"
            });
        }

        // =====================================================
        // DELETE USER (Admin ONLY)
        // =====================================================
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                bool isAdmin = User.IsInRole("Admin");
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException();
                }
                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "You are not allowed to delete this user."
                });
            }
            catch (DbUpdateException) { 
            
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot delete user due to related data. and"
                });
            }
            
        }

       
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            int userId = GetCurrentUserId();

            var result = await _userService.ChangePasswordAsync(userId, dto);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Current password is incorrect"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Password changed successfully"
            });
        }

     
        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> EmailExistsAsync(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "No user found with this email"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Email exists"
            });
        }
    }
}

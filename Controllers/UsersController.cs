// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using HealthAidAPI.DTOs;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;

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

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(PagedResult<UserDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserFilterDto filter)
        {
            try
            {
                var result = await _userService.GetUsersAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return BadRequest(new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });

            return Ok(new
            {
                success = true,
                data = user
            });
        }


        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto userDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(userDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return BadRequest(new { message = "An error occurred while creating user" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto userDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, userDto);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return BadRequest(new { message = "An error occurred while updating user" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found" });

            return NoContent();
        }

        [HttpPost("{id}/change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
            if (!result)
                return BadRequest(new { message = "Current password is incorrect or user not found" });

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deactivated successfully" });
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var result = await _userService.ActivateUserAsync(id);
            if (!result)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User activated successfully" });
        }

        [HttpGet("role/{role}")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            var exists = await _userService.EmailExistsAsync(email);
            return Ok(exists);
        }

        [HttpGet("dashboard/stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(DashboardStatsDto), 200)]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var stats = await _userService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        [HttpDelete("delete-by-email")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUserByEmail([FromQuery] string email)
        {
            try
            {
                var result = await _userService.DeleteUserByEmailAsync(email);

                if (!result)
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found with the provided email."
                    });

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully",
                    deletedEmail = email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with email {Email}", email);
                return BadRequest(new
                {
                    success = false,
                    message = "An error occurred while deleting user"
                });
            }
        }

    }
}
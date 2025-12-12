// Controllers/AuthController.cs
using HealthAidAPI.DTOs.Auth;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // 🔐 Unified method to extract UserId from JWT
        private int CurrentUserId =>
            int.TryParse(
                User.FindFirst("id")?.Value
                ?? User.FindFirst("nameid")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value,
                out var id)
            ? id : 0;

        // ============================
        // LOGIN
        // ============================
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return BadRequest(new { message = "An error occurred during login" });
            }
        }

        // ============================
        // REGISTER
        // ============================
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return BadRequest(new { message = "An error occurred during registration" });
            }
        }

        // ============================
        // REFRESH TOKEN
        // ============================
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return BadRequest(new { message = "An error occurred while refreshing token" });
            }
        }

        // ============================
        // CHANGE PASSWORD (Requires Login)
        // ============================
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = CurrentUserId;

                if (userId == 0)
                    return Unauthorized(new { message = "You must be logged in to change password" });

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

                return result
                    ? Ok(new { message = "Password changed successfully" })
                    : BadRequest(new { message = "Failed to change password" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return BadRequest(new { message = "An error occurred while changing password" });
            }
        }

        // ============================
        // FORGOT PASSWORD
        // ============================
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                await _authService.ForgotPasswordAsync(forgotPasswordDto);
                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return BadRequest(new { message = "An error occurred while processing your request" });
            }
        }

        // ============================
        // RESET PASSWORD
        // ============================
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _authService.ResetPasswordAsync(resetPasswordDto);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return BadRequest(new { message = "An error occurred while resetting password" });
            }
        }

        // ============================
        // VERIFY EMAIL
        // ============================
        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail(VerifyEmailDto verifyEmailDto)
        {
            try
            {
                await _authService.VerifyEmailAsync(verifyEmailDto);
                return Ok(new { message = "Email verified successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return BadRequest(new { message = "An error occurred while verifying email" });
            }
        }

        // ============================
        // LOGOUT (Requires Login)
        // ============================
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userId = CurrentUserId;

                if (userId == 0)
                    return Unauthorized(new { message = "You must be logged in to logout" });

                var result = await _authService.LogoutAsync(userId);

                return result
                    ? Ok(new { message = "Logged out successfully" })
                    : BadRequest(new { message = "Failed to logout" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { message = "An error occurred during logout" });
            }
        }
    }
}

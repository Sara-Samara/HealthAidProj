// Services/Implementations/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HealthAidAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status != "Deleted");

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found with email {Email}", loginDto.Email);
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Check if user is active
                if (user.Status != "Active")
                {
                    _logger.LogWarning("Login failed: User {Email} is not active. Status: {Status}",
                        loginDto.Email, user.Status);
                    throw new UnauthorizedAccessException("Account is not active. Please contact support.");
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Email}", loginDto.Email);
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Generate tokens
                var token = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User logged in successfully: {Email}", user.Email);

                return new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", loginDto.Email);
                throw;
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("Registration failed: Email already exists {Email}", registerDto.Email);
                    throw new ArgumentException("Email already exists");
                }

                // Create new user
                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                    PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
                    Phone = registerDto.Phone,
                    Country = registerDto.Country,
                    City = registerDto.City,
                    Street = registerDto.Street,
                    Role = registerDto.Role,
                    Status = "Active", // Or "Pending" if email verification is required
                    CreatedAt = DateTime.UtcNow,
                    EmailVerified = false
                };

                // Generate email verification token if needed
                user.EmailVerificationToken = GenerateRandomToken();
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send verification email
                await SendVerificationEmail(user);

                // Generate tokens
                var token = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Email} with ID {UserId}",
                    user.Email, user.Id);

                return new AuthResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", registerDto.Email);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Change password failed: User not found with ID {UserId}", userId);
                    throw new KeyNotFoundException("User not found");
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Change password failed: Invalid current password for user {Email}", user.Email);
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                // Update password
                user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Invalidate all refresh tokens
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && u.Status == "Active");

                if (user == null)
                {
                    // Don't reveal that the user doesn't exist
                    _logger.LogInformation("Password reset requested for non-existent email: {Email}",
                        forgotPasswordDto.Email);
                    return true;
                }

                // Generate reset token
                user.PasswordResetToken = GenerateRandomToken();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                // Send reset email
                await SendPasswordResetEmail(user);

                _logger.LogInformation("Password reset token generated for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for email {Email}",
                    forgotPasswordDto.Email);
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PasswordResetToken == resetPasswordDto.Token &&
                                             u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                                             u.Status == "Active");

                if (user == null)
                {
                    _logger.LogWarning("Password reset failed: Invalid or expired token");
                    throw new ArgumentException("Invalid or expired reset token");
                }

                // Update password
                user.PasswordHash = _passwordHasher.HashPassword(resetPasswordDto.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                // Invalidate all refresh tokens
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password with token {Token}", resetPasswordDto.Token);
                throw;
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken ||
                    user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Token refresh failed for user ID {UserId}", userId);
                    throw new SecurityTokenException("Invalid refresh token");
                }

                // Generate new tokens
                var newToken = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Token refreshed successfully for user: {Email}", user.Email);

                return new AuthResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmailVerificationToken == verifyEmailDto.Token &&
                                             u.EmailVerificationTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    _logger.LogWarning("Email verification failed: Invalid or expired token");
                    throw new ArgumentException("Invalid or expired verification token");
                }

                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email with token {Token}", verifyEmailDto.Token);
                throw;
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Invalidate refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User logged out: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == token);

                if (user == null) return false;

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Token revoked for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                .Replace("/", "")
                .Replace("+", "")
                .Replace("=", "");
        }

        private async Task SendVerificationEmail(User user)
        {
            // Implement email sending logic
            var verificationLink = $"{_configuration["App:BaseUrl"]}/verify-email?token={user.EmailVerificationToken}";

            var emailBody = $@"
                <h1>Verify Your Email</h1>
                <p>Hello {user.FirstName},</p>
                <p>Please click the link below to verify your email address:</p>
                <a href='{verificationLink}'>Verify Email</a>
                <p>This link will expire in 24 hours.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, "Verify Your Email", emailBody);
        }

        private async Task SendPasswordResetEmail(User user)
        {
            // Implement email sending logic
            var resetLink = $"{_configuration["App:BaseUrl"]}/reset-password?token={user.PasswordResetToken}";

            var emailBody = $@"
                <h1>Reset Your Password</h1>
                <p>Hello {user.FirstName},</p>
                <p>Please click the link below to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request a password reset, please ignore this email.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);
        }
    }
}
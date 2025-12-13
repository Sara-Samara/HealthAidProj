using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Auth;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status != "Deleted");

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (user.Status != "Active")
            {
                throw new UnauthorizedAccessException("Account is not active.");
            }

            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

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
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                EmailVerified = false
            };

            user.EmailVerificationToken = GenerateRandomToken();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && u.Status == "Active");

            if (user == null)
            {
                _logger.LogInformation("User not found for password reset: {Email}", forgotPasswordDto.Email);
                return true;
            }

  
            user.PasswordResetToken = GeneratePasswordResetToken(user);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PASSWORD RESET TOKEN GENERATED:");
            _logger.LogInformation("Email: {Email}", user.Email);
            _logger.LogInformation("Token: {Token}", user.PasswordResetToken);
            _logger.LogInformation("Expires: {Expiry}", user.PasswordResetTokenExpiry);

            await SendPasswordResetEmail(user);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == resetPasswordDto.Token &&
                                         u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                                         u.Status == "Active");

            if (user == null)
            {
                throw new ArgumentException("Invalid or expired reset token");
            }

            user.PasswordHash = _passwordHasher.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.Token);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken ||
                user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var newToken = _tokenService.GenerateToken(_mapper.Map<UserDto>(user));
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDto>(user),
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == verifyEmailDto.Token &&
                                         u.EmailVerificationTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                throw new ArgumentException("Invalid or expired verification token");
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == token);

            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
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

        private string GeneratePasswordResetToken(User user)
        {
            var tokenData = $"{user.Id}|{user.Email}|{DateTime.UtcNow.Ticks}";
            var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private bool ValidatePasswordResetToken(string token, out int userId, out string email)
        {
            userId = 0;
            email = string.Empty;

            try
            {
                var safeToken = token.Replace("-", "+").Replace("_", "/");
                while (safeToken.Length % 4 != 0)
                    safeToken += "=";

                var tokenBytes = Convert.FromBase64String(safeToken);
                var payload = Encoding.UTF8.GetString(tokenBytes);

                var parts = payload.Split('|');
                if (parts.Length != 3) return false;

                userId = int.Parse(parts[0]);
                email = parts[1];
                var timestamp = long.Parse(parts[2]);

                var tokenTime = new DateTime(timestamp);
                if (DateTime.UtcNow - tokenTime > TimeSpan.FromHours(1))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task SendVerificationEmail(User user)
        {
            var verificationLink = $"{_configuration["App:BaseUrl"]}/verify-email?token={user.EmailVerificationToken}";
            await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, verificationLink);
        }

        private async Task SendPasswordResetEmail(User user)
        {
            var token = user.PasswordResetToken;
            var resetLink = $"https://localhost:3000/reset-password?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(user.Email)}";

            await _emailService.SendPasswordResetAsync(user.Email, user.FirstName, resetLink);
        }
    }
}
// Services/Implementations/UserService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher,
            IMapper mapper, ILogger<UserService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(UserFilterDto filter)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // ================================
                // 1) Search (OR)
                // ================================
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    string s = filter.Search.ToLower();

                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(s) ||
                        u.LastName.ToLower().Contains(s) ||
                        u.Email.ToLower().Contains(s) ||
                        u.Phone.Contains(s) ||
                        u.Country.ToLower().Contains(s) ||
                        u.City.ToLower().Contains(s)
                    );
                }

                // ================================
                // 2) Filters (AND)
                // ================================
                if (!string.IsNullOrWhiteSpace(filter.Role))
                {
                    string r = filter.Role.ToLower();
                    query = query.Where(u => u.Role.ToLower().Contains(r));
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    string s = filter.Status.ToLower();
                    query = query.Where(u => u.Status.ToLower().Contains(s));
                }

                if (!string.IsNullOrWhiteSpace(filter.Country))
                {
                    string c = filter.Country.ToLower();
                    query = query.Where(u => u.Country.ToLower().Contains(c));
                }

                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    string c = filter.City.ToLower();
                    query = query.Where(u => u.City.ToLower().Contains(c));
                }

                if (filter.CreatedFrom.HasValue)
                    query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);

                if (filter.CreatedTo.HasValue)
                    query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

                // ================================
                // 3) Sorting
                // ================================
                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "email" => filter.SortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                        "createdat" => filter.SortDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                        "name" =>
                            filter.SortDesc
                            ? query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName)
                            : query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                        _ =>
                            filter.SortDesc
                            ? query.OrderByDescending(u => u.Id)
                            : query.OrderBy(u => u.Id)
                    };
                }
                else
                {
                    query = query.OrderBy(u => u.Id);
                }

                // ================================
                // 4) Pagination
                // ================================
                var totalCount = await query.CountAsync();

                var users = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(u => _mapper.Map<UserDto>(u))
                    .ToListAsync();

                return new PagedResult<UserDto>
                {
                    Items = users,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with filter");
                throw;
            }
        }



        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            if (await EmailExistsAsync(userDto.Email))
                throw new ArgumentException("Email already exists");

            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password),
                Phone = userDto.Phone,
                Country = userDto.Country,
                City = userDto.City,
                Street = userDto.Street,
                Role = userDto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created: {Email}", user.Email);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto userDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(userDto.FirstName))
                user.FirstName = userDto.FirstName;

            if (!string.IsNullOrEmpty(userDto.LastName))
                user.LastName = userDto.LastName;

            if (!string.IsNullOrEmpty(userDto.Phone))
                user.Phone = userDto.Phone;

            if (!string.IsNullOrEmpty(userDto.Country))
                user.Country = userDto.Country;

            if (!string.IsNullOrEmpty(userDto.City))
                user.City = userDto.City;

            if (!string.IsNullOrEmpty(userDto.Street))
                user.Street = userDto.Street;

            if (!string.IsNullOrEmpty(userDto.Role))
                user.Role = userDto.Role;

            if (!string.IsNullOrEmpty(userDto.Status))
                user.Status = userDto.Status;

            await _context.SaveChangesAsync();
            _logger.LogInformation("User updated: {Email}", user.Email);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User deleted: {Email}", user.Email);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Password changed for user: {Email}", user.Email);
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = "Inactive";
            await _context.SaveChangesAsync();
            _logger.LogInformation("User deactivated: {Email}", user.Email);
            return true;
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = "Active";
            await _context.SaveChangesAsync();
            _logger.LogInformation("User activated: {Email}", user.Email);
            return true;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role)
                .Select(u => _mapper.Map<UserDto>(u))
                .ToListAsync();
            return users;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.Status == "Active");
            var patients = await _context.Users.CountAsync(u => u.Role == "Patient");
            var doctors = await _context.Users.CountAsync(u => u.Role == "Doctor");
            var donors = await _context.Users.CountAsync(u => u.Role == "Donor");

            var recentUsers = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .CountAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                Patients = patients,
                Doctors = doctors,
                Donors = donors,
                RecentUsers = recentUsers
            };
        }

        public async Task<IEnumerable<DailyRegistrationDto>> GetRecentRegistrationsAsync(int days = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var registrations = await _context.Users
                .Where(u => u.CreatedAt >= startDate)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new DailyRegistrationDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return registrations;
        }

        public Task<bool> DeleteUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
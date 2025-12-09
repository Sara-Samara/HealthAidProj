// Services/Interfaces/IUserService.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserDto>> GetUsersAsync(UserFilterDto filter);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto userDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> DeactivateUserAsync(int id);
        Task<bool> ActivateUserAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<DailyRegistrationDto>> GetRecentRegistrationsAsync(int days = 7);
        Task<bool> DeleteUserByEmailAsync(string email);

    }
}
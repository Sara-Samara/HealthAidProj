
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationDto>> GetNotificationsAsync(NotificationFilterDto filter);
        Task<NotificationDto?> GetNotificationByIdAsync(int id);
        Task<IEnumerable<NotificationDto>> GetNotificationsByReceiverAsync(int receiverId);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int receiverId);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
        Task<NotificationDto> CreateNotificationByNameAsync(CreateNotificationByNameDto createNotificationDto);
        Task<NotificationDto?> UpdateNotificationAsync(int id, UpdateNotificationDto updateNotificationDto);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkMultipleAsReadAsync(MarkNotificationsAsReadDto markAsReadDto);
        Task<bool> MarkAllAsReadAsync(int receiverId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> DeleteNotificationsByReceiverAsync(int receiverId);
        Task<NotificationStatsDto> GetNotificationStatsAsync(int? receiverId = null);
        Task<int> GetUnreadCountAsync(int receiverId);
        Task<bool> SendNotificationAsync(int userId, string title, string message);
        Task<bool> SendNotificationAsync(int userId, string title, string message, string type);
        Task<bool> SendAppointmentReminderAsync(int appointmentId);
        Task<bool> SendBulkNotificationAsync(List<int> userIds, string title, string message, string type = "Info");
    }
}
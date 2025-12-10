// Services/Implementations/NotificationService.cs
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Notifications;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;


namespace HealthAidAPI.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, IMapper mapper, ILogger<NotificationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(NotificationFilterDto filter)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.Sender)
                    .Include(n => n.Receiver)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(n =>
                        n.Title.Contains(filter.Search) ||
                        n.Message.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Type))
                {
                    query = query.Where(n => n.Type == filter.Type);
                }

                if (filter.ReceiverId.HasValue)
                {
                    query = query.Where(n => n.ReceiverId == filter.ReceiverId.Value);
                }

                if (filter.SenderId.HasValue)
                {
                    query = query.Where(n => n.SenderId == filter.SenderId.Value);
                }

                if (filter.IsRead.HasValue)
                {
                    query = query.Where(n => n.IsRead == filter.IsRead.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(n => n.CreatedAt <= filter.EndDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "createdat" => filter.SortDesc ?
                        query.OrderByDescending(n => n.CreatedAt) : query.OrderBy(n => n.CreatedAt),
                    "type" => filter.SortDesc ?
                        query.OrderByDescending(n => n.Type) : query.OrderBy(n => n.Type),
                    "title" => filter.SortDesc ?
                        query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
                    "isread" => filter.SortDesc ?
                        query.OrderByDescending(n => n.IsRead) : query.OrderBy(n => n.IsRead),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(n => n.NotificationId) : query.OrderBy(n => n.NotificationId)
                };

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(n => new NotificationDto
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        SenderId = n.SenderId,
                        ReceiverId = n.ReceiverId,
                        SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : "System",
                        ReceiverName = $"{n.Receiver.FirstName} {n.Receiver.LastName}"
                    })
                    .ToListAsync();

                return new PagedResult<NotificationDto>(notifications, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications with filter");
                throw;
            }
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null) return null;

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                SenderId = notification.SenderId,
                ReceiverId = notification.ReceiverId,
                SenderName = notification.Sender != null ? $"{notification.Sender.FirstName} {notification.Sender.LastName}" : "System",
                ReceiverName = $"{notification.Receiver.FirstName} {notification.Receiver.LastName}"
            };
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsByReceiverAsync(int receiverId)
        {
            var notifications = await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .Where(n => n.ReceiverId == receiverId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    SenderId = n.SenderId,
                    ReceiverId = n.ReceiverId,
                    SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : "System",
                    ReceiverName = $"{n.Receiver.FirstName} {n.Receiver.LastName}"
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(int receiverId)
        {
            var notifications = await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .Where(n => n.ReceiverId == receiverId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    SenderId = n.SenderId,
                    ReceiverId = n.ReceiverId,
                    SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : "System",
                    ReceiverName = $"{n.Receiver.FirstName} {n.Receiver.LastName}"
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            var receiver = await _context.Users.FindAsync(createNotificationDto.ReceiverId);
            if (receiver == null)
                throw new ArgumentException($"Receiver with ID {createNotificationDto.ReceiverId} not found");

            User? sender = null;
            if (createNotificationDto.SenderId.HasValue)
            {
                sender = await _context.Users.FindAsync(createNotificationDto.SenderId.Value);
                if (sender == null)
                    throw new ArgumentException($"Sender with ID {createNotificationDto.SenderId} not found");
            }

            var notification = new Notification
            {
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                Type = createNotificationDto.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                SenderId = createNotificationDto.SenderId,
                ReceiverId = createNotificationDto.ReceiverId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification created for receiver {ReceiverId}", createNotificationDto.ReceiverId);

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                SenderId = notification.SenderId,
                ReceiverId = notification.ReceiverId,
                SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "System",
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}"
            };
        }

        public async Task<NotificationDto> CreateNotificationByNameAsync(CreateNotificationByNameDto createNotificationDto)
        {
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == createNotificationDto.ReceiverName);
            if (receiver == null)
                throw new ArgumentException($"Receiver '{createNotificationDto.ReceiverName}' not found");

            User? sender = null;
            if (!string.IsNullOrEmpty(createNotificationDto.SenderName))
            {
                sender = await _context.Users
                    .FirstOrDefaultAsync(u => u.FirstName == createNotificationDto.SenderName);
                if (sender == null)
                    throw new ArgumentException($"Sender '{createNotificationDto.SenderName}' not found");
            }

            var notification = new Notification
            {
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                Type = createNotificationDto.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                SenderId = sender?.Id,
                ReceiverId = receiver.Id
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification created for receiver {ReceiverName}", createNotificationDto.ReceiverName);

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                SenderId = notification.SenderId,
                ReceiverId = notification.ReceiverId,
                SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "System",
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}"
            };
        }

        public async Task<NotificationDto?> UpdateNotificationAsync(int id, UpdateNotificationDto updateNotificationDto)
        {
            var notification = await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateNotificationDto.Title))
                notification.Title = updateNotificationDto.Title;

            if (!string.IsNullOrEmpty(updateNotificationDto.Message))
                notification.Message = updateNotificationDto.Message;

            if (!string.IsNullOrEmpty(updateNotificationDto.Type))
                notification.Type = updateNotificationDto.Type;

            if (updateNotificationDto.IsRead.HasValue)
                notification.IsRead = updateNotificationDto.IsRead.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} updated", id);

            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                SenderId = notification.SenderId,
                ReceiverId = notification.ReceiverId,
                SenderName = notification.Sender != null ? $"{notification.Sender.FirstName} {notification.Sender.LastName}" : "System",
                ReceiverName = $"{notification.Receiver.FirstName} {notification.Receiver.LastName}"
            };
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || notification.IsRead) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} marked as read", id);
            return true;
        }

        public async Task<bool> MarkMultipleAsReadAsync(MarkNotificationsAsReadDto markAsReadDto)
        {
            IQueryable<Notification> query = _context.Notifications;

            if (markAsReadDto.NotificationIds != null && markAsReadDto.NotificationIds.Any())
            {
                query = query.Where(n => markAsReadDto.NotificationIds.Contains(n.NotificationId));
            }

            if (markAsReadDto.ReceiverId.HasValue)
            {
                query = query.Where(n => n.ReceiverId == markAsReadDto.ReceiverId.Value);
            }

            var notifications = await query.Where(n => !n.IsRead).ToListAsync();
            if (!notifications.Any()) return false;

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} notifications as read", notifications.Count);
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int receiverId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.ReceiverId == receiverId && !n.IsRead)
                .ToListAsync();

            if (!unreadNotifications.Any()) return false;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked all {Count} notifications as read for user {ReceiverId}",
                unreadNotifications.Count, receiverId);
            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification {NotificationId} deleted", id);
            return true;
        }

        public async Task<bool> DeleteNotificationsByReceiverAsync(int receiverId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ReceiverId == receiverId)
                .ToListAsync();

            if (!notifications.Any()) return false;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} notifications for receiver {ReceiverId}",
                notifications.Count, receiverId);
            return true;
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(int? receiverId = null)
        {
            var query = _context.Notifications.AsQueryable();

            if (receiverId.HasValue)
            {
                query = query.Where(n => n.ReceiverId == receiverId.Value);
            }

            var totalNotifications = await query.CountAsync();
            var unreadNotifications = await query.CountAsync(n => !n.IsRead);
            var infoCount = await query.CountAsync(n => n.Type == "Info");
            var warningCount = await query.CountAsync(n => n.Type == "Warning");
            var alertCount = await query.CountAsync(n => n.Type == "Alert");
            var successCount = await query.CountAsync(n => n.Type == "Success");
            var promotionalCount = await query.CountAsync(n => n.Type == "Promotional");

            var today = DateTime.UtcNow.Date;
            var todayNotifications = await query.CountAsync(n => n.CreatedAt >= today);

            return new NotificationStatsDto
            {
                TotalNotifications = totalNotifications,
                UnreadNotifications = unreadNotifications,
                InfoCount = infoCount,
                WarningCount = warningCount,
                AlertCount = alertCount,
                SuccessCount = successCount,
                PromotionalCount = promotionalCount,
                TodayNotifications = todayNotifications
            };
        }

        public async Task<int> GetUnreadCountAsync(int receiverId)
        {
            return await _context.Notifications
                .CountAsync(n => n.ReceiverId == receiverId && !n.IsRead);
        }

        // === الدوال الجديدة المعدلة ===

        public async Task<bool> SendNotificationAsync(int userId, string title, string message)
        {
            return await SendNotificationAsync(userId, title, message, "Info");
        }

        public async Task<bool> SendNotificationAsync(int userId, string title, string message, string type)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Cannot send notification: User with ID {UserId} not found", userId);
                    return false;
                }

                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    SenderId = null, // System notification
                    ReceiverId = userId
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SendAppointmentReminderAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning("Cannot send appointment reminder: Appointment with ID {AppointmentId} not found", appointmentId);
                    return false;
                }

                if (appointment.Status != "Scheduled" && appointment.Status != "Confirmed")
                {
                    _logger.LogWarning("Cannot send reminder for appointment {AppointmentId} with status {Status}",
                        appointmentId, appointment.Status);
                    return false;
                }

                var timeUntilAppointment = appointment.AppointmentDate - DateTime.Now;

                // Send reminder only if appointment is within 24 hours
                if (timeUntilAppointment <= TimeSpan.FromHours(24) && timeUntilAppointment > TimeSpan.Zero)
                {
                    var patientMessage = $"Reminder: You have an appointment with Dr. {appointment.Doctor.User.LastName} " +
                                       $"on {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt}";

                    var doctorMessage = $"Reminder: You have an appointment with {appointment.Patient.User.FirstName} " +
                                      $"{appointment.Patient.User.LastName} on {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt}";

                    // Send to patient - استخدام UserId مباشرة
                    await SendNotificationAsync(
                        appointment.Patient.UserId,
                        "Appointment Reminder",
                        patientMessage,
                        "Info"
                    );

                    // Send to doctor - استخدام UserId مباشرة
                    await SendNotificationAsync(
                        appointment.Doctor.UserId,
                        "Appointment Reminder",
                        doctorMessage,
                        "Info"
                    );

                    _logger.LogInformation("Appointment reminders sent for appointment {AppointmentId}", appointmentId);
                    return true;
                }

                _logger.LogInformation("Appointment {AppointmentId} is not within reminder window", appointmentId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending appointment reminder for appointment {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<bool> SendBulkNotificationAsync(List<int> userIds, string title, string message, string type = "Info")
        {
            try
            {
                var validUsers = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                if (!validUsers.Any())
                {
                    _logger.LogWarning("No valid users found for bulk notification");
                    return false;
                }

                var notifications = validUsers.Select(userId => new Notification
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    SenderId = null, // System notification
                    ReceiverId = userId
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk notification sent to {Count} users: {Title}", validUsers.Count, title);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification to {Count} users", userIds.Count);
                return false;
            }
        }
    }
}
// Services/Implementations/MessageService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Messages;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(ApplicationDbContext context, IMapper mapper, ILogger<MessageService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MessageDto>> GetAllMessagesAsync(MessageFilterDto filter)
        {
            try
            {
                var query = _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(m => m.Content.Contains(filter.Search));
                }

                if (filter.SenderId.HasValue)
                {
                    query = query.Where(m => m.SenderId == filter.SenderId.Value);
                }

                if (filter.ReceiverId.HasValue)
                {
                    query = query.Where(m => m.ReceiverId == filter.ReceiverId.Value);
                }

                if (filter.IsRead.HasValue)
                {
                    query = query.Where(m => m.IsRead == filter.IsRead.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(m => m.SentAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(m => m.SentAt <= filter.EndDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "sentat" => filter.SortDesc ?
                        query.OrderByDescending(m => m.SentAt) : query.OrderBy(m => m.SentAt),
                    "sender" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Sender.FirstName) : query.OrderBy(m => m.Sender.FirstName),
                    "receiver" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Receiver.FirstName) : query.OrderBy(m => m.Receiver.FirstName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(m => m.MessageId) : query.OrderBy(m => m.MessageId)
                };

                var totalCount = await query.CountAsync();
                var messages = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(m => new MessageDto
                    {
                        MessageId = m.MessageId,
                        Content = m.Content,
                        SentAt = m.SentAt,
                        EditedAt = m.EditedAt,
                        IsRead = m.IsRead,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                        ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}"
                    })
                    .ToListAsync();

                return new PagedResult<MessageDto>(messages, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages with filter");
                throw;
            }
        }

        public async Task<MessageDto?> GetMessageByIdAsync(int id)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null) return null;

            return new MessageDto
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SentAt = message.SentAt,
                EditedAt = message.EditedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                ReceiverName = $"{message.Receiver.FirstName} {message.Receiver.LastName}"
            };
        }

        public async Task<MessageDto> CreateMessageAsync(CreateMessageDto createMessageDto)
        {
            var sender = await _context.Users.FindAsync(createMessageDto.SenderId);
            var receiver = await _context.Users.FindAsync(createMessageDto.ReceiverId);

            if (sender == null)
                throw new ArgumentException($"Sender with ID {createMessageDto.SenderId} not found");

            if (receiver == null)
                throw new ArgumentException($"Receiver with ID {createMessageDto.ReceiverId} not found");

            var message = new Message
            {
                Content = createMessageDto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                SenderId = createMessageDto.SenderId,
                ReceiverId = createMessageDto.ReceiverId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message created from {SenderId} to {ReceiverId}",
                createMessageDto.SenderId, createMessageDto.ReceiverId);

            return new MessageDto
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}"
            };
        }

        public async Task<MessageDto> CreateMessageByNameAsync(CreateMessageByNameDto createMessageDto)
        {
            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == createMessageDto.SenderName);

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == createMessageDto.ReceiverName);

            if (sender == null)
                throw new ArgumentException($"Sender '{createMessageDto.SenderName}' not found");

            if (receiver == null)
                throw new ArgumentException($"Receiver '{createMessageDto.ReceiverName}' not found");

            var message = new Message
            {
                Content = createMessageDto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                SenderId = sender.Id,
                ReceiverId = receiver.Id
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message created from {SenderName} to {ReceiverName}",
                createMessageDto.SenderName, createMessageDto.ReceiverName);

            return new MessageDto
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}"
            };
        }

        public async Task<IEnumerable<MessageDto>> GetChatBetweenUsersAsync(ChatRequestDto chatRequest)
        {
            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == chatRequest.SenderName);

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == chatRequest.ReceiverName);

            if (sender == null)
                throw new ArgumentException($"Sender '{chatRequest.SenderName}' not found");

            if (receiver == null)
                throw new ArgumentException($"Receiver '{chatRequest.ReceiverName}' not found");

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                    (m.SenderId == receiver.Id && m.ReceiverId == sender.Id)
                )
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    EditedAt = m.EditedAt,
                    IsRead = m.IsRead,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}"
                })
                .ToListAsync();

            return messages;
        }

        public async Task<bool> MarkMessagesAsReadAsync(MarkAsReadDto markAsReadDto)
        {
            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == markAsReadDto.SenderName);

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == markAsReadDto.ReceiverName);

            if (sender == null || receiver == null)
                return false;

            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == sender.Id &&
                           m.ReceiverId == receiver.Id &&
                           m.IsRead == false)
                .ToListAsync();

            if (!unreadMessages.Any())
                return false;

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {Count} messages as read from {Sender} to {Receiver}",
                unreadMessages.Count, markAsReadDto.SenderName, markAsReadDto.ReceiverName);

            return true;
        }

        public async Task<int> GetUnreadCountAsync(string receiverName)
        {
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.FirstName == receiverName);

            if (receiver == null)
                throw new ArgumentException($"Receiver '{receiverName}' not found");

            var count = await _context.Messages
                .CountAsync(m => m.ReceiverId == receiver.Id && m.IsRead == false);

            return count;
        }

        public async Task<MessageDto?> UpdateMessageAsync(int messageId, UpdateMessageDto updateMessageDto)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            if (message == null) return null;

            message.Content = updateMessageDto.NewContent;
            message.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} updated", messageId);

            return new MessageDto
            {
                MessageId = message.MessageId,
                Content = message.Content,
                SentAt = message.SentAt,
                EditedAt = message.EditedAt,
                IsRead = message.IsRead,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = $"{message.Sender.FirstName} {message.Sender.LastName}",
                ReceiverName = $"{message.Receiver.FirstName} {message.Receiver.LastName}"
            };
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} deleted", messageId);
            return true;
        }

        public async Task<bool> DeleteAllMessagesAsync()
        {
            var messages = await _context.Messages.ToListAsync();
            if (!messages.Any()) return false;

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("All {Count} messages deleted", messages.Count);
            return true;
        }

        public async Task<bool> DeleteMessagesByUserAsync(int userId)
        {
            var userMessages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();

            if (!userMessages.Any()) return false;

            _context.Messages.RemoveRange(userMessages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} messages for user {UserId}", userMessages.Count, userId);
            return true;
        }
    }
}
// Services/Interfaces/IMessageService.cs
using HealthAidAPI.DTOs.Messages;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IMessageService
    {
        Task<PagedResult<MessageDto>> GetAllMessagesAsync(MessageFilterDto filter);
        Task<MessageDto?> GetMessageByIdAsync(int id);
        Task<MessageDto> CreateMessageAsync(CreateMessageDto createMessageDto);
        Task<MessageDto> CreateMessageByNameAsync(CreateMessageByNameDto createMessageDto);
        Task<IEnumerable<MessageDto>> GetChatBetweenUsersAsync(ChatRequestDto chatRequest);
        Task<bool> MarkMessagesAsReadAsync(MarkAsReadDto markAsReadDto);
        Task<int> GetUnreadCountAsync(string receiverName);
        Task<MessageDto?> UpdateMessageAsync(int messageId, UpdateMessageDto updateMessageDto);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<bool> DeleteAllMessagesAsync();
        Task<bool> DeleteMessagesByUserAsync(int userId);
    }
}
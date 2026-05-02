using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    public void AddMessage(Message message);
    public void DeleteMessage(Message message);
    public Task<Message?> GetMessage(string id);
    public Task<PaginatedResult<MessageDto>> GetMessagesForMember(MessageParams messageParams);
    public Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentMemberId, string recipientId);
    public Task<bool> SaveAllAsync();
}

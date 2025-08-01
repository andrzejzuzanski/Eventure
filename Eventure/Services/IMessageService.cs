using Eventure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventure.Services
{
    public interface IMessageService
    {
        Task<List<Conversation>> GetUserConversationsAsync(string userId);
        Task<Conversation> GetConversationAsync(int conversationId, string userId);
        Task<Conversation> StartOrGetConversationAsync(string senderId, string recipientId);
        Task<Message> SendMessageAsync(int conversationId, string senderId, string content);
        Task<int> GetUnreadMessagesCountAsync(string userId);
        Task MarkMessagesAsReadAsync(int conversationId, string userId);
    }
}
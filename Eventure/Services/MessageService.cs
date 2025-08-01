using Eventure.Data;
using Eventure.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventure.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation> GetConversationAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Participants.Any(p => p.UserId == userId));

            return conversation;
        }

        public async Task<int> GetUnreadMessagesCountAsync(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.SenderId != userId &&
                                 !m.IsRead &&
                                 m.Conversation.Participants.Any(p => p.UserId == userId));
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : c.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(int conversationId, string userId)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == conversationId &&
                            m.SenderId != userId &&
                            !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Message> SendMessageAsync(int conversationId, string senderId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Conversation> StartOrGetConversationAsync(string senderId, string recipientId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .Where(c => c.Participants.Count() == 2 &&
                            c.Participants.Any(p => p.UserId == senderId) &&
                            c.Participants.Any(p => p.UserId == recipientId))
                .FirstOrDefaultAsync();

            if (conversation != null)
            {
                return conversation;
            }

            var newConversation = new Conversation();
            newConversation.Participants.Add(new ConversationParticipant { UserId = senderId });
            newConversation.Participants.Add(new ConversationParticipant { UserId = recipientId });

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();
            return newConversation;
        }
    }
}
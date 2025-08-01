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
            // Pobierz konwersację, upewniając się, że zalogowany użytkownik jest jej uczestnikiem
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .ThenInclude(p => p.User) // Załaduj dane uczestników
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender) // Załaduj dane nadawców wiadomości
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Participants.Any(p => p.UserId == userId));

            return conversation;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            // Pobierz wszystkie konwersacje, w których użytkownik bierze udział
            return await _context.Conversations
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.SentAt) : c.CreatedAt) // Sortuj po dacie ostatniej wiadomości
                .ToListAsync();
        }

        public async Task<Message> SendMessageAsync(int conversationId, string senderId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false // Domyślnie nowa wiadomość jest nieprzeczytana
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Conversation> StartOrGetConversationAsync(string senderId, string recipientId)
        {
            // Znajdź konwersację, która ma DOKŁADNIE tych dwóch uczestników
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .Where(c => c.Participants.Count() == 2 &&
                            c.Participants.Any(p => p.UserId == senderId) &&
                            c.Participants.Any(p => p.UserId == recipientId))
                .FirstOrDefaultAsync();

            // Jeśli konwersacja już istnieje, zwróć ją
            if (conversation != null)
            {
                return conversation;
            }

            // Jeśli nie, stwórz nową
            var newConversation = new Conversation();
            newConversation.Participants.Add(new ConversationParticipant { UserId = senderId });
            newConversation.Participants.Add(new ConversationParticipant { UserId = recipientId });

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();
            return newConversation;
        }
    }
}
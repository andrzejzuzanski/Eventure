using Eventure.Data;
using Eventure.Models;
using Eventure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventure.Tests
{
    public class MessageServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task StartOrGetConversationAsync_ConversationDoesNotExists_ShouldReturnNewConversation()
        {

            var dbContext = GetInMemoryDbContext();
            var senderId = "sender-1";
            var recipientId = "recipient-2";

            var messageService = new MessageService(dbContext);

            var result = await messageService.StartOrGetConversationAsync(senderId, recipientId);

            Assert.NotNull(result);

            var conversationInDb = await dbContext.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync();

            Assert.NotNull(conversationInDb);
            Assert.Equal(result.Id, conversationInDb.Id);
            Assert.Equal(2, conversationInDb.Participants.Count);
            Assert.Contains(conversationInDb.Participants, p => p.UserId == senderId);
            Assert.Contains(conversationInDb.Participants, p => p.UserId == recipientId);
        }

        [Fact]
        public async Task StartOrGetConversationAsync_ConversationExists_ShouldReturnExistingConversation()
        {
            var dbContext = GetInMemoryDbContext();
            var senderId = "sender-1";
            var recipientId = "recipient-2";

            var existingConversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = senderId },
                    new ConversationParticipant { UserId = recipientId }
                }
            };

            dbContext.Conversations.Add(existingConversation);
            await dbContext.SaveChangesAsync();

            var conversationInDb = await dbContext.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync();

            Assert.NotNull(conversationInDb);

            var messageService = new MessageService(dbContext);
            var result = await messageService.StartOrGetConversationAsync(senderId, recipientId);

            Assert.NotNull(result);
            Assert.Equal(existingConversation.Id, result.Id);
            Assert.Equal(2, result.Participants.Count);
            Assert.Contains(result.Participants, p => p.UserId == senderId);
            Assert.Contains(result.Participants, p => p.UserId == recipientId);
        }
    }
}

using Eventure.Data;
using Eventure.Models;
using Eventure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Org.BouncyCastle.Crypto.Operators;

namespace Eventure.Tests
{
    public class EventServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CanUserModifyEventAsync_UserIsOrganiser_ShouldReturnTrue()
        {
            var dbContext = GetInMemoryDbContext();
            var organizerId = "organizer-123";
            var eventId = 1;

            dbContext.Events.Add(new Event { Id = eventId, OrganizerId = organizerId, Title = "Test Event" });
            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();

            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var result = await eventService.CanUserModifyEventAsync(eventId, organizerId);

            Assert.True(result);
        }

        [Fact]
        public async Task LeaveEventAsync_UserIsParticipant_ShouldRemoveParticipant()
        {
            var dbContext = GetInMemoryDbContext();
            var organiserId = "organizer-123";
            var participantId = "user-123";
            var eventId = 1;

            dbContext.Events.Add(new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = organiserId,
                Participants = new List<EventParticipant>
                {
                    new EventParticipant { UserId = participantId, EventId = eventId }
                }
            });
            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();
            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var result = await eventService.LeaveEventAsync(eventId, participantId);
            Assert.True(result.success);
            Assert.Equal("You have successfully left the event.", result.message);
        }

        [Fact]
        public async Task LeaveEventAsync_UserIsNotParticipant_ShouldReturnFalse()
        {
            var dbContext = GetInMemoryDbContext();
            var organiserId = "organizer-123";
            var participantId = "user-123";
            var eventId = 1;

            dbContext.Events.Add(new Event
            {
                Id = eventId,
                Title = "Test Event",
                OrganizerId = organiserId,
                Participants = new List<EventParticipant>
                {
                    new EventParticipant { UserId = "Not participant", EventId = eventId }
                }
            });
            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();
            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var result = await eventService.LeaveEventAsync(eventId, participantId);

            Assert.False(result.success);
            Assert.Equal("You are not a participant in this event.", result.message);
        }

        [Fact]
        public async Task CanUserModifyEventAsync_UserIsNotOrganizer_ShouldReturnFalse()
        {
            var dbContext = GetInMemoryDbContext();
            var organizerId = "organizer-123";
            var otherUserId = "other-user-456";
            var eventId = 1;

            dbContext.Events.Add(new Event { Id = eventId, OrganizerId = organizerId, Title = "Test Event" });
            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();
            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var result = await eventService.CanUserModifyEventAsync(eventId, otherUserId);

            Assert.False(result);
        }

        [Fact]
        public async Task GetRecommendedEventsAsync_ShouldReturnCorrectRecomendeations()
        {
            var dbContext = GetInMemoryDbContext();
            var categoryId = 1;
            var currentEventId = 1;
            var organserId = "organizer-123";

            dbContext.Events.AddRange(
                new Event { Id = 1, OrganizerId = organserId, Title = "Current Event", CategoryId = categoryId, StartDateTime = DateTime.UtcNow.AddDays(10) },

                new Event { Id = 2, OrganizerId = organserId, Title = "Rec 1: Football Match", CategoryId = categoryId, StartDateTime = DateTime.UtcNow.AddDays(2) },
                new Event { Id = 3, OrganizerId = organserId, Title = "Rec 2: Marathon", CategoryId = categoryId, StartDateTime = DateTime.UtcNow.AddDays(5) },

                new Event { Id = 4, OrganizerId = organserId, Title = "Wrong Category Event", CategoryId = 2, StartDateTime = DateTime.UtcNow.AddDays(3) },
                new Event { Id = 5, OrganizerId = organserId, Title = "Past Event", CategoryId = categoryId, StartDateTime = DateTime.UtcNow.AddDays(-1) }
        );

            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();
            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var result = await eventService.GetRecommendedEventsAsync(categoryId, currentEventId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, e => e.Id == currentEventId);
            Assert.DoesNotContain(result, e => e.Title == "Wrong Category Event");
            Assert.DoesNotContain(result, e => e.Title == "Past Event");
            Assert.Equal("Rec 1: Football Match", result[0].Title);
            Assert.Equal("Rec 2: Marathon", result[1].Title);
        }

        [Fact]
        public async Task UpdateEventAsync_WhenEventIsUpdated_ShouldSendNotificationsToParticipants()
        {
            var dbContext = GetInMemoryDbContext();
            var organizerId = "organizer-123";
            var participantId = "participant-456";
            var eventId = 1;
            var imageUrl = "http://example.com/image.jpg";

            var eventToUpdate = new Event
            {
                Id = eventId,
                OrganizerId = organizerId,
                Title = "Old Title",
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddHours(1),
                Participants = new List<EventParticipant>
                {
                    new EventParticipant { UserId = participantId, EventId = eventId }
                }
            };

            dbContext.Events.Add(eventToUpdate);
            await dbContext.SaveChangesAsync();

            var mockNotificationService = new Mock<INotificationService>();
            mockNotificationService.Setup(s => s.AddNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()));

            var eventService = new EventService(dbContext, mockNotificationService.Object);

            var updateVm = new ViewModels.EventCreateViewModel
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartDateTime = eventToUpdate.StartDateTime,
                EndDateTime = eventToUpdate.EndDateTime,
            };

            await eventService.UpdateEventAsync(eventId, updateVm, organizerId, imageUrl);

            mockNotificationService.Verify (s => s.AddNotificationAsync(participantId, It.IsAny<string>(), eventId),Times.Once);
        }
    }
}

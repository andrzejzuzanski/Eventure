using Eventure.Data;
using Eventure.Helpers;
using Eventure.Models;
using Eventure.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public EventService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> CanUserModifyEventAsync(int eventId, string userId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            return ev != null && ev.OrganizerId == userId;
        }

        public async Task<int> CreateEventAsync(EventCreateViewModel vm, string organizerId, string imageUrl)
        {
            var newEvent = new Event
            {
                Title = vm.Title,
                Description = vm.Description,
                StartDateTime = vm.StartDateTime,
                EndDateTime = vm.EndDateTime,
                Location = vm.Location,
                MaxParticipants = vm.MaxParticipants,
                OrganizerId = organizerId,
                CategoryId = vm.CategoryId,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ImageUrl = imageUrl
            };

            if (string.IsNullOrEmpty(newEvent.ImageUrl))
            {
                var category = await _context.Categories.FindAsync(vm.CategoryId);
                if (category != null)
                {
                    newEvent.ImageUrl = category.DefaultImageUrl;
                }
            }

            _context.Add(newEvent);
            await _context.SaveChangesAsync();

            return newEvent.Id;
        }

        public async Task<bool> DeleteEventAsAdminAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
            {
                return false;
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEventAsync(int id, string userId)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null || ev.OrganizerId != userId)
                return false;

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            categories.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Wszystkie kategorie --"
            });

            return categories;
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<Event?> GetEventWithDetailsAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .Include(e => e.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<PaginatedList<Event>> GetFilteredEventsAsync(string searchTitle, string location, DateTime? startDate, int? categoryId, int pageNumber, int pageSize)
        {
            var eventsQuery = _context.Events
                .Include(e => e.Organizer)
                .OrderBy(e => e.StartDateTime)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
                eventsQuery = eventsQuery.Where(e => EF.Functions.Like(e.Title, $"%{searchTitle}%"));

            if (!string.IsNullOrWhiteSpace(location))
                eventsQuery = eventsQuery.Where(e => EF.Functions.Like(e.Location, $"%{location}%"));

            if (startDate.HasValue)
                eventsQuery = eventsQuery.Where(e => e.StartDateTime >= startDate.Value);

            if (categoryId.HasValue)
                eventsQuery = eventsQuery.Where(e => e.CategoryId == categoryId.Value);

            return await PaginatedList<Event>
                .CreateAsync(eventsQuery.AsNoTracking(), pageNumber, pageSize);
        }

        public async Task<List<Event>> GetRecommendedEventsAsync(int categoryId, int currentEventId)
        {
            return await _context.Events
            .Where(e => e.CategoryId == categoryId &&
                          e.Id != currentEventId &&
                          e.StartDateTime > DateTime.UtcNow)
            .OrderBy(e => e.StartDateTime)
            .Take(5)
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<MyEventsViewModel> GetUserEventsAsync(string userId)
        {
            var organizedEvents = await _context.Events
                .Where(e => e.OrganizerId == userId)
                .OrderByDescending(e => e.StartDateTime)
                .AsNoTracking()
                .ToListAsync();

            var joinedEvents = await _context.EventParticipants
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                    .ThenInclude(e => e.Organizer)
                .OrderByDescending(p => p.Event.StartDateTime)
                .AsNoTracking()
                .ToListAsync();

            return new MyEventsViewModel
            {
                OrganizedEvents = organizedEvents,
                JoinedEvents = joinedEvents.Select(p => p.Event).ToList()
            };
        }

        public async Task<(bool success, string message)> JoinEventAsync(int eventId, string userId)
        {
            var ev = await _context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return (false, "Wydarzenie nie zostało znalezione.");

            bool alreadyJoined = ev.Participants.Any(p => p.UserId == userId);

            if (alreadyJoined)
                return (false, "Już bierzesz udział w tym wydarzeniu.");

            if (ev.MaxParticipants.HasValue && ev.Participants.Count >= ev.MaxParticipants)
                return (false, "Brak miejsc – wydarzenie jest pełne.");

            var participant = new EventParticipant
            {
                EventId = ev.Id,
                UserId = userId
            };

            _context.EventParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return (true, "Pomyślnie dołączono do wydarzenia.");
        }

        public async Task<(bool success, string message)> LeaveEventAsync(int eventId, string userId)
        {
            var participant = await _context.EventParticipants
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);

            if (participant == null)
                return (false, "Nie jesteś uczestnikiem tego wydarzenia.");

            _context.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            return (true, "Pomyślnie opuściłeś wydarzenie.");
        }

        public async Task<bool> UpdateEventAsync(int id, EventCreateViewModel vm, string userId, string imageUrl)
        {
            var ev = await _context.Events
                .Include(e => e.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null || ev.OrganizerId != userId)
                return false;

            var notifications = new List<string>();

            if (ev.Title != vm.Title)
                notifications.Add($"Tytuł wydarzenia został zmieniony na: {vm.Title}");

            if (ev.StartDateTime != vm.StartDateTime)
                notifications.Add($"Nowy termin rozpoczęcia: {vm.StartDateTime:dd.MM.yyyy HH:mm}");

            if (ev.EndDateTime != vm.EndDateTime)
                notifications.Add($"Nowy termin zakończenia: {vm.EndDateTime:dd.MM.yyyy HH:mm}");

            if (ev.Location != vm.Location)
                notifications.Add($"Lokalizacja wydarzenia została zmieniona na: {vm.Location}");

            ev.Title = vm.Title;
            ev.Description = vm.Description;
            ev.StartDateTime = vm.StartDateTime;
            ev.EndDateTime = vm.EndDateTime;
            ev.Location = vm.Location;
            ev.MaxParticipants = vm.MaxParticipants;
            ev.CategoryId = vm.CategoryId;
            ev.Latitude = vm.Latitude;
            ev.Longitude = vm.Longitude;
            ev.ImageUrl = imageUrl;

            if (string.IsNullOrEmpty(ev.ImageUrl))
            {
                var category = await _context.Categories.FindAsync(vm.CategoryId);
                if (category != null)
                {
                    ev.ImageUrl = category.DefaultImageUrl;
                }
            }

            await _context.SaveChangesAsync();

            if (notifications.Any() && ev.Participants.Any())
            {
                var message = string.Join("\n", notifications);

                foreach (var user in ev.Participants)
                {
                    await _notificationService.AddNotificationAsync(user.UserId, message, ev.Id);
                }
            }

            return true;
        }
    }
}

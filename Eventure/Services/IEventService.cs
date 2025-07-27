using Eventure.Helpers;
using Eventure.Models;
using Eventure.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventure.Services
{
    public interface IEventService
    {
        Task<PaginatedList<Event>> GetFilteredEventsAsync(
            string searchTitle,
            string location,
            DateTime? startDate,
            int? categoryId,
            int pageNumber,
            int pageSize);

        Task<List<SelectListItem>> GetCategorySelectListAsync();

        Task<Event?> GetEventByIdAsync(int id);

        Task<Event?> GetEventWithDetailsAsync(int id);

        Task<int> CreateEventAsync(EventCreateViewModel vm, string organizerId);

        Task<bool> UpdateEventAsync(int id, EventCreateViewModel vm, string userId);

        Task<bool> DeleteEventAsync(int id, string userId);

        Task<bool> CanUserModifyEventAsync(int eventId, string userId);

        Task<(bool success, string message)> JoinEventAsync(int eventId, string userId);

        Task<(bool success, string message)> LeaveEventAsync(int eventId, string userId);

        Task<MyEventsViewModel> GetUserEventsAsync(string userId);
        Task<bool> DeleteEventAsAdminAsync(int id);
    }
}

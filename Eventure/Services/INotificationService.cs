using Eventure.Models;

namespace Eventure.Services
{
    public interface INotificationService
    {
        Task AddNotificationAsync(string userId, string message, int? eventId = null);
        Task<List<Notification>> GetUnreadAsync(string userId);
        Task<List<Notification>> GetAllForUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
    }
}

using Eventure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Eventure.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IUserContextService _userContextService;

        public NotificationsController(INotificationService notificationService, IUserContextService userContextService)
        {
            _notificationService = notificationService;
            _userContextService = userContextService;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userContextService.GetCurrentUserAsync();
            var notifications = await _notificationService.GetAllForUserAsync(user.Id);

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var user = await _userContextService.GetCurrentUserAsync();
            var unread = await _notificationService.GetUnreadAsync(user.Id);
            return PartialView("_NotificationBell", unread.Count);
        }
    }
}

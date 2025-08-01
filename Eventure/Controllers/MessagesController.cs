using Eventure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Eventure.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserContextService _userContextService;

        public MessagesController(IMessageService messageService, IUserContextService userContextService)
        {
            _messageService = messageService;
            _userContextService = userContextService;
        }

        // Akcja wyświetlająca listę konwersacji (inbox)
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var conversations = await _messageService.GetUserConversationsAsync(currentUser.Id);
            return View(conversations);
        }

        // Akcja wyświetlająca konkretną konwersację
        public async Task<IActionResult> Conversation(int id)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var conversation = await _messageService.GetConversationAsync(id, currentUser.Id);
            if (conversation == null)
            {
                return NotFound(); // Lub Forbid(), jeśli nie ma uprawnień
            }
            return View(conversation);
        }

        // Akcja POST do wysyłania wiadomości
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Message"] = "Wiadomość nie może być pusta.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Conversation), new { id = conversationId });
            }

            var currentUser = await _userContextService.GetCurrentUserAsync();
            await _messageService.SendMessageAsync(conversationId, currentUser.Id, content);

            return RedirectToAction(nameof(Conversation), new { id = conversationId });
        }

        // Akcja do rozpoczynania konwersacji (np. z profilu innego użytkownika)
        public async Task<IActionResult> StartConversation(string recipientId)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            if (currentUser.Id == recipientId)
            {
                // Nie można pisać do samego siebie
                return RedirectToAction("Index", "Events"); // Przekieruj gdzieś indziej
            }

            var conversation = await _messageService.StartOrGetConversationAsync(currentUser.Id, recipientId);
            return RedirectToAction(nameof(Conversation), new { id = conversation.Id });
        }
    }
}
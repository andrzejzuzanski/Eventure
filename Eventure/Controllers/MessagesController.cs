using Eventure.Hubs;
using Eventure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Eventure.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IUserContextService _userContextService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(IMessageService messageService, IUserContextService userContextService, IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _userContextService = userContextService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var conversations = await _messageService.GetUserConversationsAsync(currentUser.Id);
            return View(conversations);
        }

        public async Task<IActionResult> Conversation(int id)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            await _messageService.MarkMessagesAsReadAsync(id, currentUser.Id);

            var conversation = await _messageService.GetConversationAsync(id, currentUser.Id);
            if (conversation == null)
            {
                return NotFound();
            }
            return View(conversation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Message"] = "The message cannot be empty.";
                TempData["MessageType"] = "warning";
                return RedirectToAction(nameof(Conversation), new { id = conversationId });
            }

            var currentUser = await _userContextService.GetCurrentUserAsync();
            var newMessage = await _messageService.SendMessageAsync(conversationId, currentUser.Id, content);

            await _hubContext.Clients
                .Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", new
                {
                    content = newMessage.Content,
                    senderName = currentUser.UserName,
                    sentAt = newMessage.SentAt.ToString("g")
                });

            return Ok();
        }

        public async Task<IActionResult> StartConversation(string recipientId)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            if (currentUser.Id == recipientId)
            {
                return RedirectToAction("Index", "Events");
            }

            var conversation = await _messageService.StartOrGetConversationAsync(currentUser.Id, recipientId);
            return RedirectToAction(nameof(Conversation), new { id = conversation.Id });
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var user = await _userContextService.GetCurrentUserAsync();
            if (user == null)
            {
                return NoContent();
            }

            var unreadCount = await _messageService.GetUnreadMessagesCountAsync(user.Id);
            return PartialView("_MessageBadge", unreadCount);
        }
    }
}
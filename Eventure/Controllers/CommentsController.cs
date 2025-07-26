using Eventure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventure.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IUserContextService _userContextService;

        public CommentsController(ICommentService commentService, IUserContextService userContextService)
        {
            _commentService = commentService;
            _userContextService = userContextService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int eventId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var user = await _userContextService.GetCurrentUserAsync();

            await _commentService.AddCommentAsync(eventId, user.Id, content, parentCommentId);

            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}
using Eventure.Data;
using Eventure.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        public CommentService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task AddCommentAsync(int eventId, string userId, string content, int? parentCommentId)
        {
            var newComment = new Comment
            {
                EventId = eventId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            var relevantEvent = await _context.Events
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == eventId);

            if (relevantEvent == null) return;

            if (parentCommentId == null)
            {
                if (relevantEvent.OrganizerId != userId)
                {
                    var message = $"Użytkownik {newComment.User?.UserName ?? "ktoś"} dodał nowy komentarz do Twojego wydarzenia: '{relevantEvent.Title}'.";
                    await _notificationService.AddNotificationAsync(relevantEvent.OrganizerId, message, eventId);
                }
            }
            else
            {
                var parentComment = await _context.Comments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == parentCommentId);

                if (parentComment != null && parentComment.UserId != userId)
                {
                    var message = $"Użytkownik {newComment.User?.UserName ?? "ktoś"} odpowiedział na Twój komentarz w wydarzeniu: '{relevantEvent.Title}'.";
                    await _notificationService.AddNotificationAsync(parentComment.UserId, message, eventId);
                }
            }
        }

        public async Task<List<Comment>> GetCommentsForEventAsync(int eventId)
        {
            var comments = await _context.Comments
                .Where(c => c.EventId == eventId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var commentDictionary = comments.ToDictionary(c => c.Id);
            var rootComments = new List<Comment>();

            foreach (var comment in comments)
            {
                if (comment.ParentCommentId.HasValue && commentDictionary.ContainsKey(comment.ParentCommentId.Value))
                {
                    var parent = commentDictionary[comment.ParentCommentId.Value];
                    parent.Replies.Add(comment);
                }
                else
                {
                    rootComments.Add(comment);
                }
            }

            return rootComments;
        }
    }
}
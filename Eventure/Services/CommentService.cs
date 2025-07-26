using Eventure.Data;
using Eventure.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
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
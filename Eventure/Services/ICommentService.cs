using Eventure.Models;

namespace Eventure.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetCommentsForEventAsync(int eventId);
        Task AddCommentAsync(int eventId, string userId, string content, int? parentCommentId);
    }
}

using Eventure.Models;

namespace Eventure.Services
{
    public interface IUserContextService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
    }
}

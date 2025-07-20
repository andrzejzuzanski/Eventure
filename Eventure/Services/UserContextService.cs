using Eventure.Models;
using Microsoft.AspNetCore.Identity;

namespace Eventure.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return await _userManager.GetUserAsync(user);
        }
    }
}

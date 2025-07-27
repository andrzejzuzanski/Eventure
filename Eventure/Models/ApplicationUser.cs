using Microsoft.AspNetCore.Identity;

namespace Eventure.Models

{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfilePictureUrl { get; set; }
        public ICollection<Event>? EventsOrganised { get; set; }
        public ICollection<EventParticipant> EventsParticipating { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;

namespace Eventure.Models

{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Event>? EventsOrganised { get; set; }
        public ICollection<EventParticipant> EventsParticipating { get; set; }
    }
}

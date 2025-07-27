using Eventure.Models;

namespace Eventure.ViewModels
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; }
        public List<Comment> RootComments { get; set; }
        public bool IsUserParticipant { get; set; }
        public List<Event> RecommendedEvents { get; set; }
    }
}
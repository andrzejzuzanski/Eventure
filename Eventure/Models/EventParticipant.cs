namespace Eventure.Models
{
    public class EventParticipant
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}

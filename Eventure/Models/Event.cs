using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventure.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Location { get; set; }
        public int? MaxParticipants { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Relations
        public string OrganizerId { get; set; }
        public ApplicationUser Organizer { get; set; }
        public ICollection<EventParticipant> Participants { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}

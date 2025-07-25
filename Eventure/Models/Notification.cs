﻿namespace Eventure.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        public int? EventId { get; set; }
        public Event? Event { get; set; }
    }
}

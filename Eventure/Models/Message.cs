namespace Eventure.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        // Relations
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
    }
}
namespace Eventure.Models
{
    public class ConversationParticipant
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
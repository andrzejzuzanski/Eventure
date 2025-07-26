using System.ComponentModel.DataAnnotations;

namespace Eventure.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}

using Eventure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Event>(eb =>
            {
                eb.HasOne(e => e.Organizer)
                .WithMany(u => u.EventsOrganised)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

                eb.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<EventParticipant>(eb =>
            {
                eb.HasKey(ep => new { ep.EventId, ep.UserId });

                eb.HasOne(ep => ep.Event)
                .WithMany(ep => ep.Participants)
                .HasForeignKey(ep => ep.EventId);

                eb.HasOne(ep => ep.User)
                .WithMany(u => u.EventsParticipating)
                .HasForeignKey(ep => ep.UserId);
            });

            builder.Entity<Category>(eb =>
            {
                eb.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

                eb.HasData(
                        new Category { Id = 1, Name = "Bussiness", DefaultImageUrl = "/uploads/categories/bussiness.jpg" },
                        new Category { Id = 2, Name = "Sport", DefaultImageUrl = "/uploads/categories/sport.jpg" },
                        new Category { Id = 3, Name = "Culture", DefaultImageUrl = "/uploads/categories/culture.jpg" },
                        new Category { Id = 4, Name = "Technology", DefaultImageUrl = "/uploads/categories/technology.jpg" },
                        new Category { Id = 5, Name = "Education", DefaultImageUrl = "/uploads/categories/education.jpg" }
                    );
            });

            builder.Entity<Notification>(eb =>
            {
                eb.HasKey(n => n.Id);

                eb.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

                eb.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(n => n.Event)
                .WithMany()
                .HasForeignKey(n => n.EventId)
                .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Comment>(eb =>
            {
                eb.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(c => c.Event).WithMany().HasForeignKey(c => c.EventId);

                eb.HasOne(c => c.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(c => c.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ConversationParticipant>(eb =>
            {
                eb.HasKey(cp => new { cp.ConversationId, cp.UserId });
                eb.HasOne(cp => cp.Conversation)
                    .WithMany(c => c.Participants)
                    .HasForeignKey(cp => cp.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
                eb.HasOne(cp => cp.User)
                    .WithMany()
                    .HasForeignKey(cp => cp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Message>(eb =>
            {
                eb.Property(m => m.Content).IsRequired();

                eb.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

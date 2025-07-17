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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Event>(eb =>
            {
                eb.HasOne(e => e.Organizer)
                .WithMany(u => u.EventsOrganised)
                .HasForeignKey(e => e.OrganizerId)
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
        }
    }
}

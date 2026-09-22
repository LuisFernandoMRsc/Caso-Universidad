using CampusConnect.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketComment> TicketComments { get; set; } = null!;
        public DbSet<TicketEvidence> TicketEvidences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if needed beyond attributes
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Student)
                .WithMany(u => u.SubmittedTickets)
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedSupport)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedSupportId)
                .OnDelete(DeleteBehavior.SetNull); // Set null if support user is deleted
        }
    }
}

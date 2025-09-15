using Microsoft.EntityFrameworkCore;
using quickBook.Models;

namespace quickBook.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
        public DbSet<AppointmentParticipant> AppointmentParticipants { get; set; }
        public DbSet<AppointmentRecurrence> AppointmentRecurrences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppointmentParticipant>()
                .HasKey(ap => new { ap.AppointmentId, ap.UserId });

            modelBuilder.Entity<AppointmentParticipant>()
                .HasOne(ap => ap.Appointment)
                .WithMany()
                .HasForeignKey(ap => ap.AppointmentId);

            modelBuilder.Entity<AppointmentParticipant>()
                .HasOne(ap => ap.User)
                .WithMany()
                .HasForeignKey(ap => ap.UserId);
        }
    }
}
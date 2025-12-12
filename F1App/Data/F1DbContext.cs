using Microsoft.EntityFrameworkCore;
using F1App.Models;

namespace F1App.Data
{
    public class F1DbContext : DbContext
    {
        public F1DbContext(DbContextOptions<F1DbContext> options) : base(options)
        {
        }

        public DbSet<Season> Seasons { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Circuit> Circuits { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionResult> SessionResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys and relationships here
            modelBuilder.Entity<SessionResult>()
                .HasKey(sr => new { sr.SessionId, sr.DriverId });

            modelBuilder.Entity<Season>()
                .ToTable("Season");
            
            modelBuilder.Entity<Driver>()
                .ToTable("Driver");

            modelBuilder.Entity<Team>()
                .ToTable("Team");

            modelBuilder.Entity<Circuit>()
                .ToTable("Circuit");

            modelBuilder.Entity<Event>()
                .ToTable("Event");

            modelBuilder.Entity<Session>()
                .ToTable("Session");

            modelBuilder.Entity<SessionResult>()
                .ToTable("SessionResult");
        }
    }
}

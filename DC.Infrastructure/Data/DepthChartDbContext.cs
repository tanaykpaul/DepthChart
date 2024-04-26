using DC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC.Infrastructure.Data
{
    public class DepthChartDbContext : DbContext
    {
        // Define DbSet properties for each entity
        public DbSet<Sport> Sports { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Order> Orders { get; set; }

        // Constructor to configure the database connection
        public DepthChartDbContext(DbContextOptions<DepthChartDbContext> options)
            : base(options)
        {
        }

        // Optionally configure relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many relationships
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Sport)
                .WithMany(s => s.Teams)
                .HasForeignKey(t => t.SportId);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Positions)
                .HasForeignKey(p => p.TeamId);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Position)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PositionId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Player)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PlayerId);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using KidGuard.Core.Models;

namespace KidGuard.Infrastructure.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<TimeRestriction> TimeRestrictions { get; set; }
        public DbSet<WebsiteFilter> WebsiteFilters { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=kidguard.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ActivityLog relationships
            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            // Screenshot relationships
            modelBuilder.Entity<Screenshot>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            // TimeRestriction relationships
            modelBuilder.Entity<TimeRestriction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            // WebsiteFilter relationships
            modelBuilder.Entity<WebsiteFilter>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);
        }
    }
}
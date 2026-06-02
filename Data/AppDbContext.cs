using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on Jti for fast blacklist lookups
        modelBuilder.Entity<RevokedToken>()
            .HasIndex(r => r.Jti)
            .IsUnique();

        // Index for querying a user's activity log
        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => new { a.UserId, a.Timestamp });
    }
}

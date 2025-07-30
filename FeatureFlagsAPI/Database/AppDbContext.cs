using FeatureFlagsAPI.Models;

using Microsoft.EntityFrameworkCore;

namespace FeatureFlagsAPI.Database;

public class AppDbContext : DbContext
{
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder
        .Entity<FeatureFlag>()
            .HasMany(i => i.Dependant)
            .WithMany()
            .UsingEntity<Dictionary<string, FeatureFlag>>(
                "FeatureFlagsLink",
                j => j.HasOne<FeatureFlag>().WithMany().HasForeignKey("ChildId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne<FeatureFlag>().WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Restrict)
            );
}

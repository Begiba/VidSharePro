// VidSharePro.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using VidSharePro.Domain.Common;
using VidSharePro.Domain.Entities;

namespace VidSharePro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<VideoFormat> VideoFormats => Set<VideoFormat>();
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<BackgroundJob> Jobs => Set<BackgroundJob>();
    public DbSet<VideoView> VideoViews => Set<VideoView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global Soft Delete Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private static dynamic ConvertFilterExpression(Type type)
    {
        // Equivalent to: e => !e.IsDeleted
        return (dynamic)typeof(AppDbContext)
            .GetMethod(nameof(GetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(type)
            .Invoke(null, null)!;
    }

    private static System.Linq.Expressions.LambdaExpression GetSoftDeleteFilter<TEntity>() where TEntity : BaseEntity
        => (System.Linq.Expressions.Expression<Func<TEntity, bool>>)(e => !e.IsDeleted);

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.MarkAsUpdated("System"); // Simplified for now
                    break;
                case EntityState.Modified:
                    entry.Entity.MarkAsUpdated("System");
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete();
                    break;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}
using BonusesApp.Core.Models;
using BonusesApp.Core.Models.Account;
using BonusesApp.Core.Models.Bonuses;
using BonusesApp.Core.Services.Account.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BonusesApp.Core.Infrastructure;

public class ApplicationDbContext(DbContextOptions options, IUserIdAccessor userIdAccessor) :
    IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{

    public DbSet<BonusesEntity> Bonuses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Claims)
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Roles)
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationRole>()
            .HasMany(r => r.Claims)
            .WithOne()
            .HasForeignKey(c => c.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ApplicationRole>()
            .HasMany(r => r.Users)
            .WithOne()
            .HasForeignKey(r => r.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BonusesEntity>().ToTable("Bonuses")
            .HasIndex(x => x.PhoneNumber);

        builder.Entity<BonusesEntity>()
            .HasIndex(x => x.Name);
        
    }

    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AddAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AddAuditInfo()
    {
        var currentUserId = userIdAccessor.GetCurrentUserId();

        var modifiedEntries = ChangeTracker.Entries()
            .Where(x => x.Entity is IAuditableEntity &&
                        (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entry in modifiedEntries)
        {
            var entity = (IAuditableEntity)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = now;
                entity.CreatedBy = currentUserId;
            }
            else
            {
                base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
            }

            entity.UpdatedDate = now;
            entity.UpdatedBy = currentUserId;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Tasky.NotificationService.Domain.Entities;

namespace Tasky.NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);
                
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(1000);
                
            entity.Property(e => e.Type)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.Data)
                .HasMaxLength(2000);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
        });
    }
}
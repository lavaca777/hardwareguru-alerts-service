using Alerts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Infrastructure.Persistence;

public sealed class AlertsDbContext(DbContextOptions<AlertsDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var alert = modelBuilder.Entity<Alert>();

        alert.ToTable("alerts");
        alert.HasKey(a => a.Id);

        alert.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        alert.Property(a => a.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        alert.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(100)
            .IsRequired();

        alert.Property(a => a.BuildId)
            .HasColumnName("build_id")
            .HasMaxLength(100)
            .IsRequired();

        alert.Property(a => a.BuildName)
            .HasColumnName("build_name")
            .HasMaxLength(200)
            .IsRequired();

        alert.Property(a => a.UsagePercentage)
            .HasColumnName("usage_percentage")
            .IsRequired();

        alert.Property(a => a.Message)
            .HasColumnName("message")
            .HasMaxLength(500)
            .IsRequired();

        alert.Property(a => a.Suggestion)
            .HasColumnName("suggestion")
            .HasMaxLength(500)
            .IsRequired();

        alert.Property(a => a.IsRead)
            .HasColumnName("is_read")
            .IsRequired();

        alert.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        alert.HasIndex(a => a.EventId)
            .IsUnique()
            .HasDatabaseName("ux_alerts_event_id");

        alert.HasIndex(a => new { a.UserId, a.CreatedAt })
            .HasDatabaseName("ix_alerts_user_id_created_at");
    }
}

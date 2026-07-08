using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        builder.ToTable("Alarms");
        builder.Property(a => a.Label).HasMaxLength(200).IsRequired();
        builder.Property(a => a.SoundId).HasMaxLength(50).IsRequired();
        builder.HasIndex(a => new { a.UserId, a.IsEnabled });

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AlarmHistoryConfiguration : IEntityTypeConfiguration<AlarmHistory>
{
    public void Configure(EntityTypeBuilder<AlarmHistory> builder)
    {
        builder.ToTable("AlarmHistories");
        builder.HasIndex(h => new { h.AlarmId, h.OccurredAt });

        builder.HasOne(h => h.Alarm)
            .WithMany(a => a.History)
            .HasForeignKey(h => h.AlarmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

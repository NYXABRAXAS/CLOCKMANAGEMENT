using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("CalendarEvents");
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Location).HasMaxLength(300);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.ExternalProvider).HasMaxLength(50);
        builder.Property(e => e.ExternalEventId).HasMaxLength(200);
        builder.HasIndex(e => new { e.UserId, e.StartAt });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EventCountdownConfiguration : IEntityTypeConfiguration<EventCountdown>
{
    public void Configure(EntityTypeBuilder<EventCountdown> builder)
    {
        builder.ToTable("EventCountdowns");
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Emoji).HasMaxLength(10);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.HasIndex(e => new { e.UserId, e.TargetDate });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

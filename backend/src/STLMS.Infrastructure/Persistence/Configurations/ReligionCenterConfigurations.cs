using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class FestivalCalendarEntryConfiguration : IEntityTypeConfiguration<FestivalCalendarEntry>
{
    public void Configure(EntityTypeBuilder<FestivalCalendarEntry> builder)
    {
        builder.ToTable("FestivalCalendarEntries");
        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(1000);
        builder.Property(f => f.Emoji).HasMaxLength(10);
        builder.HasIndex(f => new { f.ReligionId, f.Date });

        builder.HasOne(f => f.Religion)
            .WithMany()
            .HasForeignKey(f => f.ReligionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DailyQuoteConfiguration : IEntityTypeConfiguration<DailyQuote>
{
    public void Configure(EntityTypeBuilder<DailyQuote> builder)
    {
        builder.ToTable("DailyQuotes");
        builder.Property(q => q.Text).HasMaxLength(1000).IsRequired();
        builder.Property(q => q.Source).HasMaxLength(200);

        builder.HasOne(q => q.Religion)
            .WithMany()
            .HasForeignKey(q => q.ReligionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserPrayerLogConfiguration : IEntityTypeConfiguration<UserPrayerLog>
{
    public void Configure(EntityTypeBuilder<UserPrayerLog> builder)
    {
        builder.ToTable("UserPrayerLogs");
        builder.Property(l => l.PrayerName).HasMaxLength(20).IsRequired();
        builder.HasIndex(l => new { l.UserId, l.Date, l.PrayerName }).IsUnique();

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

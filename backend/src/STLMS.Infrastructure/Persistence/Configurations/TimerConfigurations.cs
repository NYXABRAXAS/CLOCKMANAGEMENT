using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class CountdownTimerConfiguration : IEntityTypeConfiguration<CountdownTimer>
{
    public void Configure(EntityTypeBuilder<CountdownTimer> builder)
    {
        builder.ToTable("CountdownTimers");
        builder.Property(c => c.Label).HasMaxLength(200).IsRequired();
        builder.Property(c => c.SoundId).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.UserId);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StopwatchSessionConfiguration : IEntityTypeConfiguration<StopwatchSession>
{
    public void Configure(EntityTypeBuilder<StopwatchSession> builder)
    {
        builder.ToTable("StopwatchSessions");
        builder.Property(s => s.Label).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => new { s.UserId, s.StartedAt });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StopwatchLapConfiguration : IEntityTypeConfiguration<StopwatchLap>
{
    public void Configure(EntityTypeBuilder<StopwatchLap> builder)
    {
        builder.ToTable("StopwatchLaps");
        builder.HasIndex(l => new { l.StopwatchSessionId, l.LapNumber });

        builder.HasOne(l => l.StopwatchSession)
            .WithMany(s => s.Laps)
            .HasForeignKey(l => l.StopwatchSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PomodoroSessionConfiguration : IEntityTypeConfiguration<PomodoroSession>
{
    public void Configure(EntityTypeBuilder<PomodoroSession> builder)
    {
        builder.ToTable("PomodoroSessions");
        builder.HasIndex(p => new { p.UserId, p.StartedAt });

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PomodoroLogConfiguration : IEntityTypeConfiguration<PomodoroLog>
{
    public void Configure(EntityTypeBuilder<PomodoroLog> builder)
    {
        builder.ToTable("PomodoroLogs");
        builder.HasIndex(l => new { l.PomodoroSessionId, l.StartedAt });

        builder.HasOne(l => l.PomodoroSession)
            .WithMany(p => p.Logs)
            .HasForeignKey(l => l.PomodoroSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

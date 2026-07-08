using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Dosage).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(1000);
        builder.HasIndex(m => new { m.UserId, m.IsActive });

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MedicineTimeConfiguration : IEntityTypeConfiguration<MedicineTime>
{
    public void Configure(EntityTypeBuilder<MedicineTime> builder)
    {
        builder.ToTable("MedicineTimes");
        builder.HasOne(t => t.Medicine)
            .WithMany(m => m.Times)
            .HasForeignKey(t => t.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MedicineLogConfiguration : IEntityTypeConfiguration<MedicineLog>
{
    public void Configure(EntityTypeBuilder<MedicineLog> builder)
    {
        builder.ToTable("MedicineLogs");
        builder.HasIndex(l => new { l.MedicineId, l.ScheduledDate, l.ScheduledHour, l.ScheduledMinute }).IsUnique();

        builder.HasOne(l => l.Medicine)
            .WithMany()
            .HasForeignKey(l => l.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.ToTable("Habits");
        builder.Property(h => h.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(h => new { h.UserId, h.IsActive });

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HabitLogConfiguration : IEntityTypeConfiguration<HabitLog>
{
    public void Configure(EntityTypeBuilder<HabitLog> builder)
    {
        builder.ToTable("HabitLogs");
        builder.HasIndex(l => new { l.HabitId, l.Date }).IsUnique();

        builder.HasOne(l => l.Habit)
            .WithMany(h => h.Logs)
            .HasForeignKey(l => l.HabitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SleepLogConfiguration : IEntityTypeConfiguration<SleepLog>
{
    public void Configure(EntityTypeBuilder<SleepLog> builder)
    {
        builder.ToTable("SleepLogs");
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.HasIndex(s => new { s.UserId, s.Date }).IsUnique();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements");
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.Code).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500);
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");
        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();

        builder.HasOne(ua => ua.User)
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Achievement)
            .WithMany()
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

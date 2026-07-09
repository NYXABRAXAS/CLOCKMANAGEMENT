using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices");
        builder.Property(d => d.FcmToken).HasMaxLength(4096).IsRequired();
        builder.Property(d => d.Platform).HasMaxLength(20);
        builder.HasIndex(d => new { d.UserId, d.FcmToken }).IsUnique();

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MedicineReminderLogConfiguration : IEntityTypeConfiguration<MedicineReminderLog>
{
    public void Configure(EntityTypeBuilder<MedicineReminderLog> builder)
    {
        builder.ToTable("MedicineReminderLogs");
        builder.HasIndex(l => new { l.MedicineId, l.Date, l.Hour, l.Minute }).IsUnique();

        builder.HasOne(l => l.Medicine)
            .WithMany()
            .HasForeignKey(l => l.MedicineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

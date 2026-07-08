using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasIndex(c => c.Name);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Country).HasMaxLength(100).IsRequired();
        builder.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(c => c.TimezoneId).HasMaxLength(100).IsRequired();
    }
}

public class WorldClockCityConfiguration : IEntityTypeConfiguration<WorldClockCity>
{
    public void Configure(EntityTypeBuilder<WorldClockCity> builder)
    {
        builder.ToTable("WorldClockCities");
        builder.HasIndex(w => new { w.UserId, w.CityId }).IsUnique();

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.City)
            .WithMany()
            .HasForeignKey(w => w.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using STLMS.Domain.Common;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence;

/// <summary>
/// Base context used everywhere in application code (repositories, seeders, controllers all
/// depend on this type). At runtime, DI resolves it to whichever concrete subclass matches the
/// configured provider (<see cref="SqliteAppDbContext"/> or <see cref="SqlServerAppDbContext"/>) -
/// see <see cref="DependencyInjection.AddInfrastructure"/>. The subclasses exist because EF Core
/// filters which migrations apply to a context by the [DbContext(typeof(...))] attribute each
/// generated migration carries; sharing one context type across two providers with independent
/// migration sets in the same assembly caused EF to try applying SQL-Server-flavoured migration
/// operations against SQLite (and vice versa) - confirmed by reproducing the exact
/// PendingModelChangesWarning/mismatch this design avoids.
/// </summary>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Religion> Religions => Set<Religion>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<WorldClockCity> WorldClockCities => Set<WorldClockCity>();
    public DbSet<Alarm> Alarms => Set<Alarm>();
    public DbSet<AlarmHistory> AlarmHistories => Set<AlarmHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CountdownTimer> CountdownTimers => Set<CountdownTimer>();
    public DbSet<StopwatchSession> StopwatchSessions => Set<StopwatchSession>();
    public DbSet<StopwatchLap> StopwatchLaps => Set<StopwatchLap>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();
    public DbSet<PomodoroLog> PomodoroLogs => Set<PomodoroLog>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<EventCountdown> EventCountdowns => Set<EventCountdown>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineTime> MedicineTimes => Set<MedicineTime>();
    public DbSet<MedicineLog> MedicineLogs => Set<MedicineLog>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<SleepLog> SleepLogs => Set<SleepLog>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global soft-delete filter for every ISoftDelete entity - nothing is ever hard-deleted.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    entry.Entity.ModifiedAt = now;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    // Soft delete: convert a hard delete into an update that flips IsDeleted.
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

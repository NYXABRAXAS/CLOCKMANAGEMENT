using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace STLMS.Infrastructure.Persistence;

/// <summary>Design-time factories for `dotnet ef migrations add/update`, one per provider so each
/// keeps its own independent migration history and model snapshot (see the design note on
/// <see cref="AppDbContext"/>). Usage:
/// dotnet ef migrations add X --context SqliteAppDbContext --output-dir Persistence/Migrations/Sqlite
/// dotnet ef migrations add X --context SqlServerAppDbContext --output-dir Persistence/Migrations/SqlServer
/// </summary>
public class SqliteAppDbContextFactory : IDesignTimeDbContextFactory<SqliteAppDbContext>
{
    public SqliteAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteAppDbContext>();
        optionsBuilder.UseSqlite(
            "Data Source=App_Data/stlms.dev.db",
            sqlite => sqlite.MigrationsAssembly("STLMS.Infrastructure").MigrationsHistoryTable("__EFMigrationsHistory_Sqlite"));
        return new SqliteAppDbContext(optionsBuilder.Options);
    }
}

public class SqlServerAppDbContextFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
{
    public SqlServerAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerAppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=STLMS;Trusted_Connection=True;",
            sql => sql.MigrationsAssembly("STLMS.Infrastructure").MigrationsHistoryTable("__EFMigrationsHistory_SqlServer"));
        return new SqlServerAppDbContext(optionsBuilder.Options);
    }
}

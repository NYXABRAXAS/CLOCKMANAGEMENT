using Microsoft.EntityFrameworkCore;

namespace STLMS.Infrastructure.Persistence;

/// <summary>Production provider for deployments where the user prefers a free managed Postgres
/// (e.g. Supabase, Neon) over hosting SQL Server themselves - see the design note on
/// <see cref="AppDbContext"/> for why this is a distinct type rather than reusing AppDbContext
/// directly for migrations.</summary>
public class PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options) : AppDbContext(options);

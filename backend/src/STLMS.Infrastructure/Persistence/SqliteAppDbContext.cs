using Microsoft.EntityFrameworkCore;

namespace STLMS.Infrastructure.Persistence;

/// <summary>Local dev/test provider - see the design note on <see cref="AppDbContext"/> for why
/// this exists as a distinct type instead of reusing AppDbContext directly for migrations.</summary>
public class SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options) : AppDbContext(options);

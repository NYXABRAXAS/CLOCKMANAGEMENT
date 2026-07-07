using Microsoft.EntityFrameworkCore;

namespace STLMS.Infrastructure.Persistence;

/// <summary>Production provider - see the design note on <see cref="AppDbContext"/> for why this
/// exists as a distinct type instead of reusing AppDbContext directly for migrations.</summary>
public class SqlServerAppDbContext(DbContextOptions<SqlServerAppDbContext> options) : AppDbContext(options);

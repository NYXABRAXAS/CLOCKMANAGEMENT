using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Common;
using STLMS.Domain.Interfaces;
using STLMS.Infrastructure.BackgroundServices;
using STLMS.Infrastructure.Caching;
using STLMS.Infrastructure.ExternalServices.Auth;
using STLMS.Infrastructure.ExternalServices.Email;
using STLMS.Infrastructure.ExternalServices.Religion;
using STLMS.Infrastructure.Identity;
using STLMS.Infrastructure.Persistence;
using STLMS.Infrastructure.Persistence.Repositories;
using STLMS.Infrastructure.Services;
using StackExchange.Redis;

namespace STLMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddCaching(services, configuration);

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ITotpService, TotpService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IExternalAuthValidator, GoogleAuthValidator>();
        services.AddSingleton<IExternalAuthValidator, MicrosoftAuthValidator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddHostedService<AlarmTriggerService>();

        services.AddHttpClient<IPrayerTimeProvider, AladhanPrayerTimeProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.aladhan.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddHttpClient<IHebrewCalendarProvider, HebcalProvider>(client =>
        {
            client.BaseAddress = new Uri("https://www.hebcal.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";

        // Registered as the concrete subclass so EF's migration filtering (by the
        // [DbContext(typeof(...))] attribute each migration carries) only ever sees the
        // migrations generated for the provider actually in use - see the design note on
        // AppDbContext. Everything else in the app depends on the base AppDbContext type, which
        // is mapped to resolve the same scoped instance below.
        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<SqlServerAppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SqlServer"),
                    sql => sql.MigrationsAssembly("STLMS.Infrastructure").MigrationsHistoryTable("__EFMigrationsHistory_SqlServer")));
            services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<SqlServerAppDbContext>());
        }
        else if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<PostgresAppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    npgsql => npgsql.MigrationsAssembly("STLMS.Infrastructure").MigrationsHistoryTable("__EFMigrationsHistory_Postgres")));
            services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<PostgresAppDbContext>());
        }
        else
        {
            services.AddDbContext<SqliteAppDbContext>(options =>
                options.UseSqlite(
                    configuration.GetConnectionString("Sqlite") ?? "Data Source=App_Data/stlms.dev.db",
                    sqlite => sqlite.MigrationsAssembly("STLMS.Infrastructure").MigrationsHistoryTable("__EFMigrationsHistory_Sqlite")));
            services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<SqliteAppDbContext>());
        }
    }

    /// <summary>Redis is used when configured and reachable; otherwise this falls back to an
    /// in-memory cache with a logged warning rather than failing startup. On this dev machine
    /// Redis isn't installed at all, so every local run exercises (and proves) the fallback path.</summary>
    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var useRedis = !string.IsNullOrWhiteSpace(redisConnectionString) && IsRedisReachable(redisConnectionString);

        if (useRedis)
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                Console.Error.WriteLine("[STLMS] Redis was configured but is unreachable - falling back to in-memory cache.");
            }
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
    }

    private static bool IsRedisReachable(string connectionString)
    {
        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 500;
            options.AbortOnConnectFail = true;
            using var connection = ConnectionMultiplexer.Connect(options);
            return connection.IsConnected;
        }
        catch
        {
            return false;
        }
    }
}

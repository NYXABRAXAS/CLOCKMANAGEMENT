using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace STLMS.API.IntegrationTests;

/// <summary>Boots the real Program.cs (real DI graph, real middleware pipeline, real DbSeeder run
/// on startup) against a throwaway SQLite file unique to this factory instance, so tests never
/// touch the local dev database and can run in parallel without colliding with each other.</summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"stlms-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Sqlite"] = $"Data Source={_dbPath}",
                ["ConnectionStrings:Redis"] = "",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        // SQLite pools connections process-wide, so the file can still be held open by the pool
        // even after the host (and its DbContext scopes) have been disposed - clear it first or
        // the delete throws IOException.
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try
        {
            File.Delete(_dbPath);
        }
        catch (IOException)
        {
            // Best-effort cleanup of a %TEMP% file - leaving one behind on rare contention isn't
            // worth failing the test run over.
        }
    }
}

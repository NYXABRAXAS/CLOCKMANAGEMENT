using Microsoft.Extensions.Hosting;
using STLMS.Application.Common.Interfaces;

namespace STLMS.Infrastructure.Services;

/// <summary>Saves profile photos to the local filesystem under wwwroot/uploads. Note this is NOT
/// durable on most free-tier PaaS hosts (e.g. Render's free instances have an ephemeral
/// filesystem - uploaded photos are lost on redeploy/restart). Swapping in a real object-storage
/// implementation (S3/Azure Blob/Supabase Storage) behind the same IFileStorageService interface
/// is a drop-in replacement once that's needed.</summary>
public class LocalFileStorageService(IHostEnvironment env) : IFileStorageService
{
    public async Task<string> SaveProfilePhotoAsync(Guid userId, Stream content, string fileExtension, CancellationToken ct = default)
    {
        var uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", "profile-photos");
        Directory.CreateDirectory(uploadsDir);

        var extension = fileExtension.TrimStart('.').ToLowerInvariant();
        var fileName = $"{userId}.{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        // Cache-busted so the browser picks up a changed photo immediately instead of serving a
        // stale cached image from the same URL.
        return $"/uploads/profile-photos/{fileName}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }
}

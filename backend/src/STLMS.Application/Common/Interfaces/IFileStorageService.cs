namespace STLMS.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>Saves the given content as the user's profile photo (replacing any previous one)
    /// and returns the URL the frontend can load it from.</summary>
    Task<string> SaveProfilePhotoAsync(Guid userId, Stream content, string fileExtension, CancellationToken ct = default);
}

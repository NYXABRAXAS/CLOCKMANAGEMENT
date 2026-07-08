using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Profile.Commands;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
public class ProfileController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    [RequirePermission("PROFILE", "edit")]
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await mediator.SendAsync(
            new UpdateProfileCommand(currentUser.UserId!.Value, request.FirstName, request.LastName), ct);
        return Ok(profile);
    }

    [RequirePermission("PROFILE", "edit")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(
            new ChangePasswordCommand(currentUser.UserId!.Value, request.CurrentPassword, request.NewPassword), ct);
        return Ok(new { message = "Password changed successfully." });
    }

    [RequirePermission("PROFILE", "edit")]
    [HttpPost("photo")]
    [RequestSizeLimit(MaxPhotoBytes)]
    public async Task<IActionResult> UploadPhoto(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) throw new ValidationException([new FluentValidation.Results.ValidationFailure("file", "No file was uploaded.")]);
        if (file.Length > MaxPhotoBytes) throw new ValidationException([new FluentValidation.Results.ValidationFailure("file", "Photo must be 5MB or smaller.")]);

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedPhotoExtensions.Contains(extension.ToLowerInvariant()))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("file", "Photo must be a JPG, PNG, or WEBP image.")]);
        }

        await using var stream = file.OpenReadStream();
        var profile = await mediator.SendAsync(
            new UpdateProfilePhotoCommand(currentUser.UserId!.Value, stream, extension), ct);
        return Ok(profile);
    }
}

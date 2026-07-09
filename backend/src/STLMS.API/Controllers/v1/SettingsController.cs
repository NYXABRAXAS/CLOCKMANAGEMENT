using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Settings.Commands;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/settings")]
public class SettingsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("SETTINGS", "edit")]
    [HttpPut]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest request, CancellationToken ct)
    {
        var profile = await mediator.SendAsync(
            new UpdateSettingsCommand(
                currentUser.UserId!.Value,
                request.CountryCode,
                request.TimezoneId,
                request.TimeFormat,
                request.Language,
                request.Theme,
                request.ReligionCode,
                request.PrayerLatitude,
                request.PrayerLongitude,
                request.PrayerCalculationMethod,
                request.WeatherLatitude,
                request.WeatherLongitude,
                request.EmailNotificationsEnabled,
                request.PushNotificationsEnabled),
            ct);
        return Ok(profile);
    }
}

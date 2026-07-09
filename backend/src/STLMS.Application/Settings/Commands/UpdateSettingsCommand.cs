using FluentValidation;
using FluentValidation.Results;
using STLMS.Application.Auth;
using STLMS.Application.Auth.Dtos;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Settings.Commands;

public record UpdateSettingsCommand(
    Guid UserId,
    string? CountryCode,
    string TimezoneId,
    string TimeFormat,
    string Language,
    string Theme,
    string? ReligionCode,
    double? PrayerLatitude,
    double? PrayerLongitude,
    int? PrayerCalculationMethod,
    double? WeatherLatitude,
    double? WeatherLongitude,
    bool EmailNotificationsEnabled,
    bool PushNotificationsEnabled) : IRequest<UserProfileDto>;

public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    private static readonly string[] ValidTimeFormats = ["12h", "24h"];
    private static readonly string[] ValidThemes = ["light", "dark", "system"];

    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.CountryCode).MaximumLength(2);
        RuleFor(x => x.TimezoneId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeFormat).Must(ValidTimeFormats.Contains).WithMessage("Time format must be '12h' or '24h'.");
        RuleFor(x => x.Language).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Theme).Must(ValidThemes.Contains).WithMessage("Theme must be 'light', 'dark', or 'system'.");
        RuleFor(x => x.PrayerLatitude).InclusiveBetween(-90, 90).When(x => x.PrayerLatitude.HasValue);
        RuleFor(x => x.PrayerLongitude).InclusiveBetween(-180, 180).When(x => x.PrayerLongitude.HasValue);
        RuleFor(x => x.WeatherLatitude).InclusiveBetween(-90, 90).When(x => x.WeatherLatitude.HasValue);
        RuleFor(x => x.WeatherLongitude).InclusiveBetween(-180, 180).When(x => x.WeatherLongitude.HasValue);
    }
}

public class UpdateSettingsCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateSettingsCommand, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(UpdateSettingsCommand request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct) ?? throw new NotFoundException("User", request.UserId);

        if (request.ReligionCode is null)
        {
            user.ReligionId = null;
            user.Religion = null;
        }
        else
        {
            var religion = await uow.Repository<Religion>().SingleOrDefaultAsync(r => r.Code == request.ReligionCode, ct)
                ?? throw new STLMS.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(request.ReligionCode), "Unknown religion code.")]);
            user.ReligionId = religion.Id;
            user.Religion = religion;
        }

        user.CountryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? null : request.CountryCode.Trim().ToUpperInvariant();
        user.TimezoneId = request.TimezoneId;
        user.TimeFormat = request.TimeFormat;
        user.Language = request.Language;
        user.Theme = request.Theme;
        user.PrayerLatitude = request.PrayerLatitude;
        user.PrayerLongitude = request.PrayerLongitude;
        user.PrayerCalculationMethod = request.PrayerCalculationMethod;
        user.WeatherLatitude = request.WeatherLatitude;
        user.WeatherLongitude = request.WeatherLongitude;
        user.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
        user.PushNotificationsEnabled = request.PushNotificationsEnabled;

        uow.Repository<User>().Update(user);
        await uow.SaveChangesAsync(ct);

        var (roles, permissions) = await UserAccessLoader.LoadAsync(uow, user.Id, ct);
        return AuthMapping.ToProfileDto(user, roles, permissions);
    }
}

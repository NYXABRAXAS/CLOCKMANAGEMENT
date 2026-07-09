using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.PrayerTimes.Commands;

public record LogPrayerCommand(Guid UserId, DateOnly Date, string PrayerName, bool Completed) : IRequest<bool>;

public class LogPrayerCommandValidator : AbstractValidator<LogPrayerCommand>
{
    private static readonly string[] ValidPrayers = ["Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"];

    public LogPrayerCommandValidator()
    {
        RuleFor(x => x.PrayerName).Must(ValidPrayers.Contains).WithMessage("Unknown prayer name.");
    }
}

public class LogPrayerCommandHandler(IUnitOfWork uow) : IRequestHandler<LogPrayerCommand, bool>
{
    public async Task<bool> HandleAsync(LogPrayerCommand request, CancellationToken ct)
    {
        var existing = await uow.Repository<UserPrayerLog>().SingleOrDefaultAsync(
            l => l.UserId == request.UserId && l.Date == request.Date && l.PrayerName == request.PrayerName, ct);

        if (existing is null)
        {
            await uow.Repository<UserPrayerLog>().AddAsync(
                new UserPrayerLog { UserId = request.UserId, Date = request.Date, PrayerName = request.PrayerName, Completed = request.Completed }, ct);
        }
        else
        {
            existing.Completed = request.Completed;
            uow.Repository<UserPrayerLog>().Update(existing);
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.SleepLogs.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Enums;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.SleepLogs.Commands;

public record SaveSleepLogCommand(
    Guid UserId, DateOnly Date, DateTime BedTime, DateTime WakeTime, SleepQuality? Quality, string? Notes) : IRequest<SleepLogDto>;

public class SaveSleepLogCommandValidator : AbstractValidator<SaveSleepLogCommand>
{
    public SaveSleepLogCommandValidator()
    {
        RuleFor(x => x.WakeTime).GreaterThan(x => x.BedTime);
    }
}

public class SaveSleepLogCommandHandler(IUnitOfWork uow) : IRequestHandler<SaveSleepLogCommand, SleepLogDto>
{
    public async Task<SleepLogDto> HandleAsync(SaveSleepLogCommand request, CancellationToken ct)
    {
        var durationMinutes = (int)(request.WakeTime - request.BedTime).TotalMinutes;

        // Upserts on (UserId, Date) - logging sleep for a night you've already logged replaces it
        // rather than erroring on the unique index.
        var existing = await uow.Repository<SleepLog>().SingleOrDefaultAsync(l => l.UserId == request.UserId && l.Date == request.Date, ct);

        SleepLog log;
        if (existing is null)
        {
            log = new SleepLog
            {
                UserId = request.UserId,
                Date = request.Date,
                BedTime = request.BedTime,
                WakeTime = request.WakeTime,
                DurationMinutes = durationMinutes,
                Quality = request.Quality,
                Notes = request.Notes,
            };
            await uow.Repository<SleepLog>().AddAsync(log, ct);
        }
        else
        {
            existing.BedTime = request.BedTime;
            existing.WakeTime = request.WakeTime;
            existing.DurationMinutes = durationMinutes;
            existing.Quality = request.Quality;
            existing.Notes = request.Notes;
            uow.Repository<SleepLog>().Update(existing);
            log = existing;
        }

        await uow.SaveChangesAsync(ct);
        return new SleepLogDto(log.Id, log.Date, log.BedTime, log.WakeTime, log.DurationMinutes, log.Quality?.ToString(), log.Notes);
    }
}

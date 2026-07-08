using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.StopwatchSessions.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.StopwatchSessions.Commands;

public record LapInput(int LapNumber, long LapDurationMs, long CumulativeDurationMs);

public record SaveStopwatchSessionCommand(
    Guid UserId,
    string Label,
    DateTime StartedAt,
    DateTime EndedAt,
    long TotalDurationMs,
    IReadOnlyList<LapInput> Laps) : IRequest<StopwatchSessionDto>;

public class SaveStopwatchSessionCommandValidator : AbstractValidator<SaveStopwatchSessionCommand>
{
    public SaveStopwatchSessionCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalDurationMs).GreaterThan(0);
        RuleFor(x => x.EndedAt).GreaterThanOrEqualTo(x => x.StartedAt);
    }
}

public class SaveStopwatchSessionCommandHandler(IUnitOfWork uow) : IRequestHandler<SaveStopwatchSessionCommand, StopwatchSessionDto>
{
    public async Task<StopwatchSessionDto> HandleAsync(SaveStopwatchSessionCommand request, CancellationToken ct)
    {
        var session = new STLMS.Domain.Entities.StopwatchSession
        {
            UserId = request.UserId,
            Label = request.Label.Trim(),
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            TotalDurationMs = request.TotalDurationMs,
        };
        await uow.Repository<STLMS.Domain.Entities.StopwatchSession>().AddAsync(session, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var lap in request.Laps)
        {
            await uow.Repository<StopwatchLap>().AddAsync(
                new StopwatchLap
                {
                    StopwatchSessionId = session.Id,
                    LapNumber = lap.LapNumber,
                    LapDurationMs = lap.LapDurationMs,
                    CumulativeDurationMs = lap.CumulativeDurationMs,
                },
                ct);
        }
        await uow.SaveChangesAsync(ct);

        return new StopwatchSessionDto(
            session.Id, session.Label, session.StartedAt, session.EndedAt, session.TotalDurationMs,
            request.Laps.Select(l => new StopwatchLapDto(l.LapNumber, l.LapDurationMs, l.CumulativeDurationMs)).ToList());
    }
}

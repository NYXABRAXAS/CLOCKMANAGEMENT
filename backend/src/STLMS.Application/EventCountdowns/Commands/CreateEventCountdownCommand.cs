using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.EventCountdowns.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.EventCountdowns.Commands;

public record CreateEventCountdownCommand(Guid UserId, string Title, DateOnly TargetDate, string? Emoji, string? Color) : IRequest<EventCountdownDto>;

public class CreateEventCountdownCommandValidator : AbstractValidator<CreateEventCountdownCommand>
{
    public CreateEventCountdownCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public class CreateEventCountdownCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateEventCountdownCommand, EventCountdownDto>
{
    public async Task<EventCountdownDto> HandleAsync(CreateEventCountdownCommand request, CancellationToken ct)
    {
        var countdown = new EventCountdown
        {
            UserId = request.UserId,
            Title = request.Title.Trim(),
            TargetDate = request.TargetDate,
            Emoji = request.Emoji,
            Color = request.Color,
        };
        await uow.Repository<EventCountdown>().AddAsync(countdown, ct);
        await uow.SaveChangesAsync(ct);
        return new EventCountdownDto(countdown.Id, countdown.Title, countdown.TargetDate, countdown.Emoji, countdown.Color);
    }
}

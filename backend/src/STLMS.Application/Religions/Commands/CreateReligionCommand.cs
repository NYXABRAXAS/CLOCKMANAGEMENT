using FluentValidation;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Religions.Queries;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Religions.Commands;

public record CreateReligionCommand(string Code, string Name, int SortOrder) : IRequest<ReligionDto>;

public class CreateReligionCommandValidator : AbstractValidator<CreateReligionCommand>
{
    public CreateReligionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[A-Z_]+$").WithMessage("Code must be uppercase letters and underscores only.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateReligionCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<CreateReligionCommand, ReligionDto>
{
    public async Task<ReligionDto> HandleAsync(CreateReligionCommand request, CancellationToken ct)
    {
        var existing = await uow.Repository<Religion>().SingleOrDefaultAsync(r => r.Code == request.Code, ct);
        if (existing is not null)
        {
            throw new STLMS.Application.Common.Exceptions.ConflictException($"A religion with code {request.Code} already exists.");
        }

        var religion = new Religion { Code = request.Code, Name = request.Name, SortOrder = request.SortOrder, IsSystem = false };
        await uow.Repository<Religion>().AddAsync(religion, ct);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("CREATE", "Religion", religion.Id, newValue: new { religion.Code, religion.Name }, ct: ct);

        return new ReligionDto(religion.Id, religion.Code, religion.Name, religion.SortOrder);
    }
}

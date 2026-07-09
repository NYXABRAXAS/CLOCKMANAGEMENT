using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Religions.Queries;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Religions.Commands;

public record UpdateReligionCommand(Guid Id, string Name, int SortOrder) : IRequest<ReligionDto>;

public class UpdateReligionCommandValidator : AbstractValidator<UpdateReligionCommand>
{
    public UpdateReligionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateReligionCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<UpdateReligionCommand, ReligionDto>
{
    public async Task<ReligionDto> HandleAsync(UpdateReligionCommand request, CancellationToken ct)
    {
        var religion = await uow.Repository<Religion>().GetByIdAsync(request.Id, ct) ?? throw new NotFoundException("Religion", request.Id);
        var oldName = religion.Name;

        religion.Name = request.Name;
        religion.SortOrder = request.SortOrder;
        uow.Repository<Religion>().Update(religion);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("UPDATE", "Religion", religion.Id, new { Name = oldName }, new { religion.Name }, ct: ct);

        return new ReligionDto(religion.Id, religion.Code, religion.Name, religion.SortOrder);
    }
}

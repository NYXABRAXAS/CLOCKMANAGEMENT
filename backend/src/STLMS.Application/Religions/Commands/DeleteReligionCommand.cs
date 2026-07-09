using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Religions.Commands;

public record DeleteReligionCommand(Guid Id) : IRequest<bool>;

public class DeleteReligionCommandHandler(IUnitOfWork uow, IAuditService auditService) : IRequestHandler<DeleteReligionCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteReligionCommand request, CancellationToken ct)
    {
        var religion = await uow.Repository<Religion>().GetByIdAsync(request.Id, ct) ?? throw new NotFoundException("Religion", request.Id);
        if (religion.IsSystem)
        {
            throw new ConflictException("Built-in religions can't be deleted.");
        }

        uow.Repository<Religion>().Remove(religion);
        await uow.SaveChangesAsync(ct);

        await auditService.LogAsync("DELETE", "Religion", religion.Id, oldValue: new { religion.Code, religion.Name }, ct: ct);

        return true;
    }
}

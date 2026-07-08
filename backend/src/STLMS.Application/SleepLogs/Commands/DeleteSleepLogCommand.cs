using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.SleepLogs.Commands;

public record DeleteSleepLogCommand(Guid UserId, Guid SleepLogId) : IRequest<bool>;

public class DeleteSleepLogCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteSleepLogCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteSleepLogCommand request, CancellationToken ct)
    {
        var log = await uow.Repository<SleepLog>().GetByIdAsync(request.SleepLogId, ct);
        if (log is null || log.UserId != request.UserId) throw new NotFoundException("SleepLog", request.SleepLogId);

        uow.Repository<SleepLog>().Remove(log);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

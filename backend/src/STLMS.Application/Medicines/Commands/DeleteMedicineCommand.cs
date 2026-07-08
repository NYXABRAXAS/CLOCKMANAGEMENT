using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Commands;

public record DeleteMedicineCommand(Guid UserId, Guid MedicineId) : IRequest<bool>;

public class DeleteMedicineCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteMedicineCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteMedicineCommand request, CancellationToken ct)
    {
        var medicine = await uow.Repository<Medicine>().GetByIdAsync(request.MedicineId, ct);
        if (medicine is null || medicine.UserId != request.UserId) throw new NotFoundException("Medicine", request.MedicineId);

        uow.Repository<Medicine>().Remove(medicine);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

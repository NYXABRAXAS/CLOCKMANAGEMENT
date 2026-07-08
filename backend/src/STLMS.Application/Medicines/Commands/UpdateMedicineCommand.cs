using FluentValidation;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Medicines.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Commands;

public record UpdateMedicineCommand(
    Guid UserId,
    Guid MedicineId,
    string Name,
    string? Dosage,
    string? Notes,
    DateOnly StartDate,
    DateOnly? EndDate,
    int RepeatDaysMask,
    bool IsActive,
    IReadOnlyList<MedicineTimeInput> Times) : IRequest<MedicineDto>;

public class UpdateMedicineCommandValidator : AbstractValidator<UpdateMedicineCommand>
{
    public UpdateMedicineCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Times).NotEmpty().WithMessage("At least one reminder time is required.");
    }
}

public class UpdateMedicineCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateMedicineCommand, MedicineDto>
{
    public async Task<MedicineDto> HandleAsync(UpdateMedicineCommand request, CancellationToken ct)
    {
        var medicine = await uow.Repository<Medicine>().GetByIdAsync(request.MedicineId, ct);
        if (medicine is null || medicine.UserId != request.UserId) throw new NotFoundException("Medicine", request.MedicineId);

        medicine.Name = request.Name.Trim();
        medicine.Dosage = request.Dosage;
        medicine.Notes = request.Notes;
        medicine.StartDate = request.StartDate;
        medicine.EndDate = request.EndDate;
        medicine.RepeatDaysMask = request.RepeatDaysMask;
        medicine.IsActive = request.IsActive;
        uow.Repository<Medicine>().Update(medicine);

        var existingTimes = await uow.Repository<MedicineTime>().FindAsync(t => t.MedicineId == medicine.Id, ct);
        foreach (var existing in existingTimes) uow.Repository<MedicineTime>().Remove(existing);
        await uow.SaveChangesAsync(ct);

        var newTimes = new List<MedicineTime>();
        foreach (var t in request.Times)
        {
            var time = new MedicineTime { MedicineId = medicine.Id, Hour = t.Hour, Minute = t.Minute };
            newTimes.Add(time);
            await uow.Repository<MedicineTime>().AddAsync(time, ct);
        }
        await uow.SaveChangesAsync(ct);

        return MedicineMapping.ToDto(medicine, newTimes);
    }
}

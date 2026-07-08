using FluentValidation;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Medicines.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Medicines.Commands;

public record MedicineTimeInput(int Hour, int Minute);

public record CreateMedicineCommand(
    Guid UserId,
    string Name,
    string? Dosage,
    string? Notes,
    DateOnly StartDate,
    DateOnly? EndDate,
    int RepeatDaysMask,
    IReadOnlyList<MedicineTimeInput> Times) : IRequest<MedicineDto>;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Times).NotEmpty().WithMessage("At least one reminder time is required.");
    }
}

public class CreateMedicineCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateMedicineCommand, MedicineDto>
{
    public async Task<MedicineDto> HandleAsync(CreateMedicineCommand request, CancellationToken ct)
    {
        var medicine = new Medicine
        {
            UserId = request.UserId,
            Name = request.Name.Trim(),
            Dosage = request.Dosage,
            Notes = request.Notes,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RepeatDaysMask = request.RepeatDaysMask,
        };
        await uow.Repository<Medicine>().AddAsync(medicine, ct);
        await uow.SaveChangesAsync(ct);

        var times = new List<MedicineTime>();
        foreach (var t in request.Times)
        {
            var time = new MedicineTime { MedicineId = medicine.Id, Hour = t.Hour, Minute = t.Minute };
            times.Add(time);
            await uow.Repository<MedicineTime>().AddAsync(time, ct);
        }
        await uow.SaveChangesAsync(ct);

        return MedicineMapping.ToDto(medicine, times);
    }
}

using STLMS.Application.Common.Mediator;
using STLMS.Application.Panchang.Dtos;

namespace STLMS.Application.Panchang.Queries;

public record GetPanchangQuery(DateOnly Date) : IRequest<PanchangDto>;

public class GetPanchangQueryHandler : IRequestHandler<GetPanchangQuery, PanchangDto>
{
    public Task<PanchangDto> HandleAsync(GetPanchangQuery request, CancellationToken ct)
    {
        var result = STLMS.Application.ReligionCalculators.PanchangCalculator.Calculate(request.Date);
        return Task.FromResult(new PanchangDto(result.TithiNumber, result.TithiName, result.Paksha, result.NakshatraNumber, result.NakshatraName, result.IsApproximate));
    }
}

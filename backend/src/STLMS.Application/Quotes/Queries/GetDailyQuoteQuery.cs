using STLMS.Application.Common.Mediator;
using STLMS.Application.Quotes.Dtos;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Quotes.Queries;

public record GetDailyQuoteQuery(Guid UserId) : IRequest<DailyQuoteDto?>;

public class GetDailyQuoteQueryHandler(IUnitOfWork uow) : IRequestHandler<GetDailyQuoteQuery, DailyQuoteDto?>
{
    public async Task<DailyQuoteDto?> HandleAsync(GetDailyQuoteQuery request, CancellationToken ct)
    {
        var user = await uow.Repository<User>().GetByIdAsync(request.UserId, ct);

        var candidates = user?.ReligionId is { } religionId
            ? await uow.Repository<DailyQuote>().FindAsync(q => q.ReligionId == religionId || q.ReligionId == null, ct)
            : await uow.Repository<DailyQuote>().FindAsync(q => q.ReligionId == null, ct);
        if (candidates.Count == 0) return null;

        // Deterministic pick by day-of-year so the "quote of the day" is stable across requests
        // within the same day but rotates daily, without needing to persist which quote was shown.
        var ordered = candidates.OrderBy(q => q.Id).ToList();
        var index = DateTime.UtcNow.DayOfYear % ordered.Count;
        var quote = ordered[index];

        return new DailyQuoteDto(quote.Text, quote.Source);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Quotes.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/quotes")]
[RequirePermission("PRAYER_CENTER", "view")]
public class QuotesController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var quote = await mediator.SendAsync(new GetDailyQuoteQuery(currentUser.UserId!.Value), ct);
        return Ok(quote);
    }
}

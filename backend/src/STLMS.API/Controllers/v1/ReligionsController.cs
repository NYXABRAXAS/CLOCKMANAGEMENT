using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Religions.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/religions")]
[RequirePermission("RELIGIONS", "view")]
public class ReligionsController(IAppMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReligions(CancellationToken ct)
    {
        var religions = await mediator.SendAsync(new GetReligionsQuery(), ct);
        return Ok(religions);
    }
}

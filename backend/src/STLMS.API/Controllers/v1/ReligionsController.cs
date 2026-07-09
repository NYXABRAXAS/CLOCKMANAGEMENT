using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Religions.Commands;
using STLMS.Application.Religions.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/religions")]
public class ReligionsController(IAppMediator mediator) : ControllerBase
{
    [RequirePermission("RELIGIONS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetReligions(CancellationToken ct)
    {
        var religions = await mediator.SendAsync(new GetReligionsQuery(), ct);
        return Ok(religions);
    }

    [RequirePermission("RELIGIONS", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateReligion(CreateReligionRequest request, CancellationToken ct)
    {
        var religion = await mediator.SendAsync(new CreateReligionCommand(request.Code, request.Name, request.SortOrder), ct);
        return Ok(religion);
    }

    [RequirePermission("RELIGIONS", "edit")]
    [HttpPut("{religionId:guid}")]
    public async Task<IActionResult> UpdateReligion(Guid religionId, UpdateReligionRequest request, CancellationToken ct)
    {
        var religion = await mediator.SendAsync(new UpdateReligionCommand(religionId, request.Name, request.SortOrder), ct);
        return Ok(religion);
    }

    [RequirePermission("RELIGIONS", "delete")]
    [HttpDelete("{religionId:guid}")]
    public async Task<IActionResult> DeleteReligion(Guid religionId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteReligionCommand(religionId), ct);
        return Ok(new { success = true });
    }
}

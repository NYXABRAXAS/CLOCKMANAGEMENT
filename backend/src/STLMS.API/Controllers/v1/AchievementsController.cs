using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.Application.Achievements.Queries;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/achievements")]
public class AchievementsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("HEALTH", "view")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var achievements = await mediator.SendAsync(new GetUserAchievementsQuery(currentUser.UserId!.Value), ct);
        return Ok(achievements);
    }
}

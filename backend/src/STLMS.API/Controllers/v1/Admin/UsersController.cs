using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Admin.Users.Commands;
using STLMS.Application.Admin.Users.Queries;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;

namespace STLMS.API.Controllers.v1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/users")]
public class UsersController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("USERS", "view")]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.SendAsync(new GetUsersQuery(search, page, pageSize), ct);
        return Ok(result);
    }

    [RequirePermission("USERS", "edit")]
    [HttpPut("{userId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid userId, SetUserActiveRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new SetUserActiveCommand(currentUser.UserId!.Value, userId, request.IsActive), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("USERS", "edit")]
    [HttpPost("{userId:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid userId, CancellationToken ct)
    {
        await mediator.SendAsync(new UnlockUserCommand(currentUser.UserId!.Value, userId), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("USERS", "edit")]
    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> AssignRole(Guid userId, AssignUserRoleRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new AssignUserRoleCommand(currentUser.UserId!.Value, userId, request.RoleCode), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("USERS", "edit")]
    [HttpPut("{userId:guid}/subscription")]
    public async Task<IActionResult> SetSubscription(Guid userId, SetUserSubscriptionRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new SetUserSubscriptionCommand(currentUser.UserId!.Value, userId, request.SubscriptionStatus, request.ExpiresAt), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("USERS", "view")]
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var file = await mediator.SendAsync(new ExportUsersQuery(), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}

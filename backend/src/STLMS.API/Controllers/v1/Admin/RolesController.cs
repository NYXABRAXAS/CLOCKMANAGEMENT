using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Admin.Roles.Commands;
using STLMS.Application.Admin.Roles.Queries;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;

namespace STLMS.API.Controllers.v1.Admin;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
public class RolesController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [RequirePermission("ROLES", "view")]
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var roles = await mediator.SendAsync(new GetRolesQuery(), ct);
        return Ok(roles);
    }

    [RequirePermission("ROLES", "view")]
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var permissions = await mediator.SendAsync(new GetPermissionsQuery(), ct);
        return Ok(permissions);
    }

    [RequirePermission("ROLES", "edit")]
    [HttpPut("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> SetRolePermission(Guid roleId, Guid permissionId, SetRolePermissionRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new SetRolePermissionCommand(currentUser.UserId!.Value, roleId, permissionId, request.Granted), ct);
        return Ok(new { success = true });
    }
}

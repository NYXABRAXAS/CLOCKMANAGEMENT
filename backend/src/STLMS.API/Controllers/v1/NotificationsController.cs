using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Notifications.Commands;
using STLMS.Application.Notifications.Queries;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[RequirePermission("NOTIFICATIONS", "view")]
public class NotificationsController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var notifications = await mediator.SendAsync(new GetNotificationsQuery(currentUser.UserId!.Value), ct);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await mediator.SendAsync(new GetUnreadNotificationCountQuery(currentUser.UserId!.Value), ct);
        return Ok(new { count });
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken ct)
    {
        await mediator.SendAsync(new MarkNotificationReadCommand(currentUser.UserId!.Value, notificationId), ct);
        return Ok(new { success = true });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await mediator.SendAsync(new MarkAllNotificationsReadCommand(currentUser.UserId!.Value), ct);
        return Ok(new { success = true });
    }

    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice(RegisterDeviceRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(new RegisterDeviceCommand(currentUser.UserId!.Value, request.FcmToken, request.Platform), ct);
        return Ok(new { success = true });
    }
}

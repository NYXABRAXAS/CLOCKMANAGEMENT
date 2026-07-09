using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using STLMS.API.Authorization;
using STLMS.API.Controllers.v1.Requests;
using STLMS.Application.Common.Exceptions;
using STLMS.Application.Common.Interfaces;
using STLMS.Application.Common.Mediator;
using STLMS.Application.Medicines.Commands;
using STLMS.Application.Medicines.Queries;
using STLMS.Domain.Enums;

namespace STLMS.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/medicines")]
public class MedicinesController(IAppMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    private static MedicineLogStatus ParseStatus(string value) =>
        Enum.TryParse<MedicineLogStatus>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ValidationException([new FluentValidation.Results.ValidationFailure("status", "Unknown dose status.")]);

    [RequirePermission("HEALTH", "view")]
    [HttpGet]
    public async Task<IActionResult> GetMedicines(CancellationToken ct)
    {
        var medicines = await mediator.SendAsync(new GetMedicinesQuery(currentUser.UserId!.Value), ct);
        return Ok(medicines);
    }

    [RequirePermission("HEALTH", "view")]
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] DateOnly date, CancellationToken ct)
    {
        var logs = await mediator.SendAsync(new GetMedicineLogsQuery(currentUser.UserId!.Value, date), ct);
        return Ok(logs);
    }

    [RequirePermission("HEALTH", "create")]
    [HttpPost]
    public async Task<IActionResult> CreateMedicine(CreateMedicineRequest request, CancellationToken ct)
    {
        var created = await mediator.SendAsync(
            new CreateMedicineCommand(
                currentUser.UserId!.Value, request.Name, request.Dosage, request.Notes, request.StartDate, request.EndDate,
                request.RepeatDaysMask, request.Times.Select(t => new MedicineTimeInput(t.Hour, t.Minute)).ToList()),
            ct);
        return Ok(created);
    }

    [RequirePermission("HEALTH", "edit")]
    [HttpPut("{medicineId:guid}")]
    public async Task<IActionResult> UpdateMedicine(Guid medicineId, UpdateMedicineRequest request, CancellationToken ct)
    {
        var updated = await mediator.SendAsync(
            new UpdateMedicineCommand(
                currentUser.UserId!.Value, medicineId, request.Name, request.Dosage, request.Notes, request.StartDate, request.EndDate,
                request.RepeatDaysMask, request.IsActive, request.Times.Select(t => new MedicineTimeInput(t.Hour, t.Minute)).ToList()),
            ct);
        return Ok(updated);
    }

    [RequirePermission("HEALTH", "edit")]
    [HttpPost("{medicineId:guid}/log")]
    public async Task<IActionResult> LogDose(Guid medicineId, LogMedicineDoseRequest request, CancellationToken ct)
    {
        await mediator.SendAsync(
            new LogMedicineDoseCommand(
                currentUser.UserId!.Value, medicineId, request.ScheduledDate, request.ScheduledHour, request.ScheduledMinute, ParseStatus(request.Status)),
            ct);
        return Ok(new { success = true });
    }

    [RequirePermission("HEALTH", "delete")]
    [HttpDelete("{medicineId:guid}")]
    public async Task<IActionResult> DeleteMedicine(Guid medicineId, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteMedicineCommand(currentUser.UserId!.Value, medicineId), ct);
        return Ok(new { success = true });
    }

    [RequirePermission("HEALTH", "view")]
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var file = await mediator.SendAsync(new ExportMedicineLogsQuery(currentUser.UserId!.Value), ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}

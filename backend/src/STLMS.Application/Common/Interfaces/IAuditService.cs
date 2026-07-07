namespace STLMS.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValue = null,
        object? newValue = null,
        string? description = null,
        CancellationToken ct = default);
}

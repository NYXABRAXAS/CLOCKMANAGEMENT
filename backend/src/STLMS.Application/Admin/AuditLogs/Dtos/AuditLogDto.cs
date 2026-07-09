namespace STLMS.Application.Admin.AuditLogs.Dtos;

public record AuditLogDto(
    Guid Id,
    Guid? ActorId,
    string? ActorEmail,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Description,
    string? IpAddress,
    DateTime CreatedAt);

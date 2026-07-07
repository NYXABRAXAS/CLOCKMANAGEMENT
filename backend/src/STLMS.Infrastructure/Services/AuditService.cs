using System.Text.Json;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.Services;

public class AuditService(IUnitOfWork uow, ICurrentUserService currentUser) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };

    public async Task LogAsync(
        string action, string entityType, Guid? entityId = null, object? oldValue = null, object? newValue = null,
        string? description = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            ActorId = currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValueJson = oldValue is null ? null : JsonSerializer.Serialize(oldValue, JsonOptions),
            NewValueJson = newValue is null ? null : JsonSerializer.Serialize(newValue, JsonOptions),
            Description = description,
            IpAddress = currentUser.IpAddress,
            UserAgent = currentUser.UserAgent,
        };

        await uow.Repository<AuditLog>().AddAsync(log, ct);
        await uow.SaveChangesAsync(ct);
    }
}

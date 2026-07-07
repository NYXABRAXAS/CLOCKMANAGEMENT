namespace STLMS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    bool HasPermission(string module, string action);
}

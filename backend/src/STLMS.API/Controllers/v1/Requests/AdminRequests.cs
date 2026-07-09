namespace STLMS.API.Controllers.v1.Requests;

public record SetUserActiveRequest(bool IsActive);

public record AssignUserRoleRequest(string RoleCode);

public record SetUserSubscriptionRequest(string SubscriptionStatus, DateTime? ExpiresAt);

public record SetRolePermissionRequest(bool Granted);

public record CreateReligionRequest(string Code, string Name, int SortOrder);

public record UpdateReligionRequest(string Name, int SortOrder);

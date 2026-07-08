namespace STLMS.API.Controllers.v1.Requests;

public record UpdateProfileRequest(string FirstName, string LastName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

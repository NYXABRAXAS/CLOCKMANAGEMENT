namespace STLMS.API.Controllers.v1.Requests;

public record RegisterDeviceRequest(string FcmToken, string? Platform);

namespace STLMS.Application.Notifications.Dtos;

public record NotificationDto(Guid Id, string Title, string Message, bool IsRead, DateTime CreatedAt);

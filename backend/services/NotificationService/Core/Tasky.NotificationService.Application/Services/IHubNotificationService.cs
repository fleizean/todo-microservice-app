using Tasky.NotificationService.Application.DTOs.Responses;

namespace Tasky.NotificationService.Application.Services;

public interface IHubNotificationService
{
    Task SendNotificationToUserAsync(string userId, NotificationResponse notification);
    Task SendNotificationCountUpdateAsync(string userId, int unreadCount);
}
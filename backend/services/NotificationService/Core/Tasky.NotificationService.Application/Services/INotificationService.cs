using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.DTOs.Responses;

namespace Tasky.NotificationService.Application.Services;

public interface INotificationService
{
    Task CreateNotificationFromTodoCreatedAsync(TodoCreatedEventDto eventDto);
    Task CreateNotificationFromTodoCompletedAsync(TodoCompletedEventDto eventDto);
    Task CreateNotificationFromTodoDeletedAsync(TodoDeletedEventDto eventDto);
    Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, bool? isRead = null);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
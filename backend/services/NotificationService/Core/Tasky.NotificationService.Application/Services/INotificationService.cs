using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.DTOs.Responses;

namespace Tasky.NotificationService.Application.Services;

public interface INotificationService
{
    Task CreateNotificationFromTodoCreatedAsync(TodoCreatedEventDto eventDto);
    Task CreateNotificationFromTodoCompletedAsync(TodoCompletedEventDto eventDto);
    Task CreateNotificationFromTodoDeletedAsync(TodoDeletedEventDto eventDto);
    Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(string userId, int page = 1, int pageSize = 10, bool? isRead = null, int? type = null);
    Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, bool? isRead = null);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
    Task MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task DeleteNotificationAsync(int notificationId);
}
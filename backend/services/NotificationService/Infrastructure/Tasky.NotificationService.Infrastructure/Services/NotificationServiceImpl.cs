using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.DTOs.Responses;
using Tasky.NotificationService.Application.Mappers;
using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubNotificationService _hubNotificationService;

    public NotificationServiceImpl(INotificationRepository notificationRepository, IHubNotificationService hubNotificationService)
    {
        _notificationRepository = notificationRepository;
        _hubNotificationService = hubNotificationService;
    }

    public async Task CreateNotificationFromTodoCreatedAsync(TodoCreatedEventDto eventDto)
    {
        var notification = NotificationMapper.FromTodoCreatedEvent(eventDto);
        var createdNotification = await _notificationRepository.CreateAsync(notification);
        
        // Send to frontend via SignalR
        var notificationResponse = NotificationMapper.ToResponse(createdNotification);
        await _hubNotificationService.SendNotificationToUserAsync(eventDto.UserId, notificationResponse);
    }

    public async Task CreateNotificationFromTodoCompletedAsync(TodoCompletedEventDto eventDto)
    {
        var notification = NotificationMapper.FromTodoCompletedEvent(eventDto);
        var createdNotification = await _notificationRepository.CreateAsync(notification);
        
        // Send to frontend via SignalR
        var notificationResponse = NotificationMapper.ToResponse(createdNotification);
        await _hubNotificationService.SendNotificationToUserAsync(eventDto.UserId, notificationResponse);
    }

    public async Task CreateNotificationFromTodoDeletedAsync(TodoDeletedEventDto eventDto)
    {
        var notification = NotificationMapper.FromTodoDeletedEvent(eventDto);
        var createdNotification = await _notificationRepository.CreateAsync(notification);
        
        // Send to frontend via SignalR
        var notificationResponse = NotificationMapper.ToResponse(createdNotification);
        await _hubNotificationService.SendNotificationToUserAsync(eventDto.UserId, notificationResponse);
    }

    public async Task<NotificationListResponse> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, bool? isRead = null)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(
            userId, 
            (page - 1) * pageSize, 
            pageSize, 
            isRead
        );
        
        var totalCount = await _notificationRepository.GetCountByUserIdAsync(userId, isRead);
        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new NotificationListResponse
        {
            Notifications = notifications.Select(NotificationMapper.ToResponse).ToList(),
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _notificationRepository.GetByIdAndUserIdAsync(notificationId, userId);
        if (notification == null || notification.IsRead) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        
        await _notificationRepository.UpdateAsync(notification);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
    }

    public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(string userId, int page = 1, int pageSize = 10, bool? isRead = null, int? type = null)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(
            userId, 
            (page - 1) * pageSize, 
            pageSize, 
            isRead,
            type
        );
        
        return notifications.Select(NotificationMapper.ToResponse);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.IsRead) return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        
        await _notificationRepository.UpdateAsync(notification);
    }


    public async Task DeleteNotificationAsync(int notificationId)
    {
        await _notificationRepository.DeleteAsync(notificationId);
    }
}
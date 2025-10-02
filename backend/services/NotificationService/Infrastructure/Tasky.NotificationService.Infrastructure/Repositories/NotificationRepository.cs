using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Domain.Entities;

namespace Tasky.NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly List<Notification> _notifications = new();
    private int _nextId = 1;

    public Task<Notification> CreateAsync(Notification notification)
    {
        notification.Id = _nextId++;
        notification.CreatedAt = DateTime.UtcNow;
        _notifications.Add(notification);
        return Task.FromResult(notification);
    }

    public Task<Notification?> GetByIdAndUserIdAsync(int id, string userId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id && n.UserId == userId);
        return Task.FromResult(notification);
    }

    public Task<List<Notification>> GetByUserIdAsync(string userId, int skip, int take, bool? isRead = null)
    {
        var query = _notifications.Where(n => n.UserId == userId);
        
        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);
            
        var result = query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
            
        return Task.FromResult(result);
    }

    public Task<int> GetCountByUserIdAsync(string userId, bool? isRead = null)
    {
        var query = _notifications.Where(n => n.UserId == userId);
        
        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);
            
        return Task.FromResult(query.Count());
    }

    public Task<int> GetUnreadCountByUserIdAsync(string userId)
    {
        var count = _notifications.Count(n => n.UserId == userId && !n.IsRead);
        return Task.FromResult(count);
    }

    public Task<Notification> UpdateAsync(Notification notification)
    {
        var existing = _notifications.FirstOrDefault(n => n.Id == notification.Id);
        if (existing != null)
        {
            existing.IsRead = notification.IsRead;
            existing.ReadAt = notification.ReadAt;
        }
        return Task.FromResult(notification);
    }

    public Task<bool> MarkAllAsReadAsync(string userId)
    {
        var userNotifications = _notifications.Where(n => n.UserId == userId && !n.IsRead);
        foreach (var notification in userNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        return Task.FromResult(true);
    }
}
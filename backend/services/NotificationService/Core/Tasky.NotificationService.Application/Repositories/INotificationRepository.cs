using Tasky.NotificationService.Domain.Entities;

namespace Tasky.NotificationService.Application.Repositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<List<Notification>> GetByUserIdAsync(string userId, int skip = 0, int take = 10, bool? isRead = null);
    Task<int> GetCountByUserIdAsync(string userId, bool? isRead = null);
    Task<int> GetUnreadCountByUserIdAsync(string userId);
    Task<Notification?> GetByIdAndUserIdAsync(int id, string userId);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> MarkAllAsReadAsync(string userId);
}
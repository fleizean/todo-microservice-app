using Microsoft.EntityFrameworkCore;
using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Domain.Entities;
using Tasky.NotificationService.Infrastructure.Data;

namespace Tasky.NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetByIdAndUserIdAsync(int id, string userId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<List<Notification>> GetByUserIdAsync(string userId, int skip, int take, bool? isRead = null, int? type = null)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);
        
        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);
            
        if (type.HasValue)
            query = query.Where(n => (int)n.Type == type.Value);
            
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(string userId, bool? isRead = null)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);
        
        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);
            
        return await query.CountAsync();
    }

    public async Task<int> GetUnreadCountByUserIdAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var userNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
            
        foreach (var notification in userNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task DeleteAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
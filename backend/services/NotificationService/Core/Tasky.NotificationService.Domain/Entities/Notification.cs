using System.ComponentModel.DataAnnotations;

namespace Tasky.NotificationService.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReadAt { get; set; }
    
    public string? Data { get; set; } // JSON format for additional data
}

public enum NotificationType
{
    TodoCreated = 1,
    TodoCompleted = 2,
    TodoDeleted = 3,
    System = 4
}
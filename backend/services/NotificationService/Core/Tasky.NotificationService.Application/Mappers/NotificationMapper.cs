using System.Text.Json;
using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.DTOs.Responses;
using Tasky.NotificationService.Domain.Entities;

namespace Tasky.NotificationService.Application.Mappers;

public static class NotificationMapper
{
    public static Notification FromTodoCreatedEvent(TodoCreatedEventDto eventDto)
    {
        return new Notification
        {
            UserId = eventDto.UserId,
            Title = "Yeni Todo Eklendi",
            Message = $"'{eventDto.Title}' başlıklı yeni bir todo oluşturdunuz.",
            Type = NotificationType.TodoCreated,
            CreatedAt = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(new { TodoId = eventDto.TodoId, Title = eventDto.Title })
        };
    }

    public static Notification FromTodoCompletedEvent(TodoCompletedEventDto eventDto)
    {
        return new Notification
        {
            UserId = eventDto.UserId,
            Title = "Todo Tamamlandı",
            Message = $"'{eventDto.Title}' başlıklı todo'yu tamamladınız. Tebrikler!",
            Type = NotificationType.TodoCompleted,
            CreatedAt = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(new { TodoId = eventDto.TodoId, Title = eventDto.Title })
        };
    }

    public static Notification FromTodoDeletedEvent(TodoDeletedEventDto eventDto)
    {
        return new Notification
        {
            UserId = eventDto.UserId,
            Title = "Todo Silindi",
            Message = $"'{eventDto.Title}' başlıklı todo silindi.",
            Type = NotificationType.TodoDeleted,
            CreatedAt = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(new { TodoId = eventDto.TodoId, Title = eventDto.Title })
        };
    }

    public static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Data = notification.Data
        };
    }
}
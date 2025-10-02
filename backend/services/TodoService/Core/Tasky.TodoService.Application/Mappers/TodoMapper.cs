using Tasky.TodoService.Application.DTOs.Events;
using Tasky.TodoService.Application.DTOs.Requests;
using Tasky.TodoService.Application.DTOs.Responses;
using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Application.Mappers;

public static class TodoMapper
{
    public static Todo ToEntity(CreateTodoRequest request, string userId)
    {
        return new Todo
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Category = request.Category,
            DueDate = request.DueDate,
            ReminderDateTime = request.ReminderDateTime,
            Notes = request.Notes,
            Tags = request.Tags,
            EstimatedMinutes = request.EstimatedMinutes,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static TodoResponse ToResponse(Todo todo)
    {
        return new TodoResponse
        {
            Id = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            Priority = todo.Priority,
            Category = todo.Category,
            DueDate = todo.DueDate,
            ReminderDateTime = todo.ReminderDateTime,
            ReminderSent = todo.ReminderSent,
            Notes = todo.Notes,
            Tags = todo.Tags,
            EstimatedMinutes = todo.EstimatedMinutes,
            IsOverdue = todo.IsOverdue,
            IsUrgent = todo.IsUrgent,
            HasReminder = todo.HasReminder,
            PriorityDisplay = todo.PriorityDisplay,
            CategoryDisplay = todo.CategoryDisplay,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            CompletedAt = todo.CompletedAt
        };
    }

    public static TodoCreatedEvent ToCreatedEvent(Todo todo)
    {
        return new TodoCreatedEvent
        {
            TodoId = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Description = todo.Description,
            CreatedAt = todo.CreatedAt
        };
    }

    public static TodoCompletedEvent ToCompletedEvent(Todo todo)
    {
        return new TodoCompletedEvent
        {
            TodoId = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            CompletedAt = todo.CompletedAt ?? DateTime.UtcNow
        };
    }

    public static TodoDeletedEvent ToDeletedEvent(Todo todo)
    {
        return new TodoDeletedEvent
        {
            TodoId = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            DeletedAt = todo.DeletedAt ?? DateTime.UtcNow
        };
    }
}
using Tasky.TodoService.Application.DTOs.Responses;

namespace Tasky.TodoService.Application.Services;

public interface IReminderService
{
    Task ProcessPendingRemindersAsync();
    Task SendReminderEmailAsync(string userEmail, TodoResponse todo);
    Task<List<TodoResponse>> GetTodosWithPendingRemindersAsync();
    Task MarkReminderAsSentAsync(int todoId);
    Task ScheduleReminderAsync(int todoId, DateTime reminderDateTime);
}
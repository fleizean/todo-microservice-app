using System.Net.Mail;
using System.Net;
using Tasky.TodoService.Application.DTOs.Responses;
using Tasky.TodoService.Application.Repositories;
using Tasky.TodoService.Application.Services;
using Tasky.TodoService.Application.Mappers;

namespace Tasky.TodoService.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly ITodoRepository _todoRepository;

    public ReminderService(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task ProcessPendingRemindersAsync()
    {
        try
        {
            var todosWithPendingReminders = await GetTodosWithPendingRemindersAsync();
            
            foreach (var todo in todosWithPendingReminders)
            {
                if (todo.ReminderDateTime <= DateTime.UtcNow && !todo.ReminderSent)
                {
                    // TODO: Get user email from user service
                    var userEmail = "user@example.com"; // Placeholder
                    
                    await SendReminderEmailAsync(userEmail, todo);
                    await MarkReminderAsSentAsync(todo.Id);
                    
                    Console.WriteLine($"Reminder sent for todo {todo.Id}: {todo.Title}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing pending reminders: {ex.Message}");
        }
    }

    public async Task<List<TodoResponse>> GetTodosWithPendingRemindersAsync()
    {
        var todos = await _todoRepository.GetTodosWithPendingRemindersAsync();
        return todos.Select(TodoMapper.ToResponse).ToList();
    }

    public async Task MarkReminderAsSentAsync(int todoId)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId);
        if (todo != null)
        {
            todo.ReminderSent = true;
            todo.UpdatedAt = DateTime.UtcNow;
            await _todoRepository.UpdateAsync(todo);
        }
    }

    public async Task ScheduleReminderAsync(int todoId, DateTime reminderDateTime)
    {
        var todo = await _todoRepository.GetByIdAsync(todoId);
        if (todo != null)
        {
            todo.ReminderDateTime = reminderDateTime;
            todo.ReminderSent = false;
            todo.UpdatedAt = DateTime.UtcNow;
            await _todoRepository.UpdateAsync(todo);
        }
    }

    public async Task SendReminderEmailAsync(string userEmail, TodoResponse todo)
    {
        try
        {
            // Simplified email sending - in production, use proper email service
            Console.WriteLine($"📧 Reminder email would be sent to {userEmail} for todo: {todo.Title}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send reminder email for todo {todo.Id}: {ex.Message}");
            throw;
        }
    }

}
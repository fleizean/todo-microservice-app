using System.Net.Mail;
using System.Net;
using System.Text.Json;
using Tasky.TodoService.Application.DTOs.Responses;
using Tasky.TodoService.Application.Repositories;
using Tasky.TodoService.Application.Services;
using Tasky.TodoService.Application.Mappers;

namespace Tasky.TodoService.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly ITodoRepository _todoRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReminderService(ITodoRepository todoRepository, IHttpClientFactory httpClientFactory)
    {
        _todoRepository = todoRepository;
        _httpClientFactory = httpClientFactory;
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
                    var userEmail = await GetUserEmailAsync(todo.UserId);
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        await SendReminderEmailAsync(userEmail, todo);
                        await MarkReminderAsSentAsync(todo.Id);
                        
                        Console.WriteLine($"Reminder sent for todo {todo.Id}: {todo.Title} to {userEmail}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get user email for todo {todo.Id}, user ID: {todo.UserId}");
                    }
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

    private async Task<string?> GetUserEmailAsync(string userId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AuthService");
            httpClient.DefaultRequestHeaders.Add("X-Service-Key", "tasky-service-internal-key");
            var response = await httpClient.GetAsync($"/api/auth/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<UserResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return userResponse?.Email;
            }
            else
            {
                Console.WriteLine($"Failed to get user info from AuthService. Status: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling AuthService for user {userId}: {ex.Message}");
            return null;
        }
    }
}

public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
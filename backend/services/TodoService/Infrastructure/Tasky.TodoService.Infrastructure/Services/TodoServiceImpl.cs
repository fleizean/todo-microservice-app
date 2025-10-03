using Tasky.TodoService.Application.DTOs.Requests;
using Tasky.TodoService.Application.DTOs.Responses;
using Tasky.TodoService.Application.Mappers;
using Tasky.TodoService.Application.Repositories;
using Tasky.TodoService.Application.Services;
using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Infrastructure.Services;

public class TodoServiceImpl : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly IEventService _eventService;

    public TodoServiceImpl(ITodoRepository todoRepository, IEventService eventService)
    {
        _todoRepository = todoRepository;
        _eventService = eventService;
    }

    public async Task<TodoResponse> CreateTodoAsync(string userId, CreateTodoRequest request)
    {
        var todo = TodoMapper.ToEntity(request, userId);
        var createdTodo = await _todoRepository.CreateAsync(todo);
        
        // Event publish
        var todoEvent = TodoMapper.ToCreatedEvent(createdTodo);
        await _eventService.PublishTodoCreatedEventAsync(todoEvent);
        
        return TodoMapper.ToResponse(createdTodo);
    }

    public async Task<TodoResponse?> GetTodoByIdAsync(int id, string userId)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(id, userId);
        return todo != null ? TodoMapper.ToResponse(todo) : null;
    }

    public async Task<TodoListResponse> GetTodosAsync(string userId, GetTodosRequest request)
    {
        var todos = await _todoRepository.GetByUserIdAsync(
            userId, 
            (request.Page - 1) * request.PageSize, 
            request.PageSize,
            request.IsCompleted,
            request.SearchTerm,
            request.SortBy ?? "CreatedAt",
            request.SortDescending
        );
        
        var totalCount = await _todoRepository.GetCountByUserIdAsync(userId, request.IsCompleted, request.SearchTerm);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new TodoListResponse
        {
            Todos = todos.Select(TodoMapper.ToResponse).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<TodoResponse?> UpdateTodoAsync(int id, string userId, UpdateTodoRequest request)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(id, userId);
        if (todo == null) return null;

        if (!string.IsNullOrEmpty(request.Title))
            todo.Title = request.Title;
            
        if (request.Description != null)
            todo.Description = request.Description;
            
        if (request.IsCompleted.HasValue)
        {
            todo.IsCompleted = request.IsCompleted.Value;
            if (request.IsCompleted.Value)
                todo.CompletedAt = DateTime.UtcNow;
            else
                todo.CompletedAt = null;
        }
        
        if (request.Priority.HasValue)
            todo.Priority = request.Priority.Value;
            
        if (request.Category.HasValue)
            todo.Category = request.Category.Value;
            
        if (request.DueDate != null)
            todo.DueDate = request.DueDate;
            
        if (request.ReminderDateTime != null)
        {
            todo.ReminderDateTime = request.ReminderDateTime;
            todo.ReminderSent = false; // Reset reminder when updated
        }
            
        if (request.Notes != null)
            todo.Notes = request.Notes;
            
        if (request.Tags != null)
            todo.Tags = request.Tags;
            
        if (request.EstimatedMinutes.HasValue)
            todo.EstimatedMinutes = request.EstimatedMinutes.Value;
        
        todo.UpdatedAt = DateTime.UtcNow;
        
        var updatedTodo = await _todoRepository.UpdateAsync(todo);
        
        // Event publish
        if (request.IsCompleted.HasValue && request.IsCompleted.Value)
        {
            var completedEvent = TodoMapper.ToCompletedEvent(updatedTodo);
            await _eventService.PublishTodoCompletedEventAsync(completedEvent);
        }
        
        return TodoMapper.ToResponse(updatedTodo);
    }

    public async Task<bool> ToggleTodoAsync(int id, string userId, ToggleTodoRequest request)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(id, userId);
        if (todo == null) return false;

        todo.IsCompleted = request.IsCompleted;
        todo.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
        todo.UpdatedAt = DateTime.UtcNow;
        
        await _todoRepository.UpdateAsync(todo);
        
        // Event publish
        if (request.IsCompleted)
        {
            var completedEvent = TodoMapper.ToCompletedEvent(todo);
            await _eventService.PublishTodoCompletedEventAsync(completedEvent);
        }
        
        return true;
    }

    public async Task<bool> DeleteTodoAsync(int id, string userId)
    {
        var todo = await _todoRepository.GetByIdAndUserIdAsync(id, userId);
        if (todo == null) return false;

        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;
        todo.UpdatedAt = DateTime.UtcNow;
        
        await _todoRepository.UpdateAsync(todo);
        
        // Event publish
        var deletedEvent = TodoMapper.ToDeletedEvent(todo);
        await _eventService.PublishTodoDeletedEventAsync(deletedEvent);
        
        return true;
    }

    public async Task<int> GetTodoCountAsync(string userId)
    {
        return await _todoRepository.GetCountByUserIdAsync(userId);
    }
}
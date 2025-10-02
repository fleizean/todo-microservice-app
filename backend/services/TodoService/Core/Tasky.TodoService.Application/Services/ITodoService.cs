using Tasky.TodoService.Application.DTOs.Requests;
using Tasky.TodoService.Application.DTOs.Responses;

namespace Tasky.TodoService.Application.Services;

public interface ITodoService
{
    Task<TodoResponse> CreateTodoAsync(string userId, CreateTodoRequest request);
    Task<TodoResponse?> GetTodoByIdAsync(int id, string userId);
    Task<TodoListResponse> GetTodosAsync(string userId, GetTodosRequest request);
    Task<TodoResponse?> UpdateTodoAsync(int id, string userId, UpdateTodoRequest request);
    Task<bool> ToggleTodoAsync(int id, string userId, ToggleTodoRequest request);
    Task<bool> DeleteTodoAsync(int id, string userId);
    Task<int> GetTodoCountAsync(string userId);
}
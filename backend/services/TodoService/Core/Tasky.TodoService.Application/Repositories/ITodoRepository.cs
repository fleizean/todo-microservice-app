using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Application.Repositories;

public interface ITodoRepository
{
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo?> GetByIdAsync(int id);
    Task<Todo?> GetByIdAndUserIdAsync(int id, string userId);
    Task<List<Todo>> GetByUserIdAsync(string userId, int skip = 0, int take = 10, bool? isCompleted = null, string? searchTerm = null, string sortBy = "CreatedAt", bool sortDescending = true);
    Task<int> GetCountByUserIdAsync(string userId, bool? isCompleted = null, string? searchTerm = null);
    Task<Todo> UpdateAsync(Todo todo);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> ExistsAsync(int id, string userId);
    
    // Enhanced queries for new features
    Task<List<Todo>> GetTodosWithPendingRemindersAsync();
    Task<List<Todo>> GetOverdueTodosAsync(string userId);
    Task<List<Todo>> GetTodosByCategoryAsync(string userId, TodoCategory category);
    Task<List<Todo>> GetTodosByPriorityAsync(string userId, TodoPriority priority);
    Task<List<Todo>> GetTodosDueInRangeAsync(string userId, DateTime startDate, DateTime endDate);
}
using Microsoft.EntityFrameworkCore;
using Tasky.TodoService.Application.Repositories;
using Tasky.TodoService.Domain.Entities;
using Tasky.TodoService.Persistence.Context;

namespace Tasky.TodoService.Persistence.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> GetByIdAsync(int id)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<Todo?> GetByIdAndUserIdAsync(int id, string userId)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
    }

    public async Task<List<Todo>> GetByUserIdAsync(string userId, int skip = 0, int take = 10, bool? isCompleted = null, string? searchTerm = null, string sortBy = "CreatedAt", bool sortDescending = true)
    {
        var query = _context.Todos
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (isCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(t => t.Title.Contains(searchTerm) || (t.Description != null && t.Description.Contains(searchTerm)));

        query = sortBy.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "updatedat" => sortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        return await query.Skip(skip).Take(take).ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(string userId, bool? isCompleted = null, string? searchTerm = null)
    {
        var query = _context.Todos
            .Where(t => t.UserId == userId && !t.IsDeleted);

        if (isCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(t => t.Title.Contains(searchTerm) || (t.Description != null && t.Description.Contains(searchTerm)));

        return await query.CountAsync();
    }

    public async Task<Todo> UpdateAsync(Todo todo)
    {
        _context.Todos.Update(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var todo = await GetByIdAndUserIdAsync(id, userId);
        if (todo == null) return false;

        todo.IsDeleted = true;
        todo.DeletedAt = DateTime.UtcNow;
        todo.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id, string userId)
    {
        return await _context.Todos
            .AnyAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted);
    }

    public async Task<List<Todo>> GetTodosWithPendingRemindersAsync()
    {
        return await _context.Todos
            .Where(t => !t.IsDeleted && 
                       t.ReminderDateTime.HasValue && 
                       t.ReminderDateTime <= DateTime.UtcNow && 
                       !t.ReminderSent &&
                       !t.IsCompleted)
            .ToListAsync();
    }

    public async Task<List<Todo>> GetOverdueTodosAsync(string userId)
    {
        return await _context.Todos
            .Where(t => t.UserId == userId && 
                       !t.IsDeleted && 
                       !t.IsCompleted &&
                       t.DueDate.HasValue && 
                       t.DueDate < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<List<Todo>> GetTodosByCategoryAsync(string userId, TodoCategory category)
    {
        return await _context.Todos
            .Where(t => t.UserId == userId && !t.IsDeleted && t.Category == category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Todo>> GetTodosByPriorityAsync(string userId, TodoPriority priority)
    {
        return await _context.Todos
            .Where(t => t.UserId == userId && !t.IsDeleted && t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Todo>> GetTodosDueInRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Todos
            .Where(t => t.UserId == userId && 
                       !t.IsDeleted && 
                       !t.IsCompleted &&
                       t.DueDate.HasValue && 
                       t.DueDate >= startDate && 
                       t.DueDate <= endDate)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }
}
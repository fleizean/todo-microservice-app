using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Application.DTOs.Responses;

public class TodoResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    
    // Enhanced fields
    public TodoPriority Priority { get; set; }
    public TodoCategory Category { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDateTime { get; set; }
    public bool ReminderSent { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public int EstimatedMinutes { get; set; }
    
    // Computed properties
    public bool IsOverdue { get; set; }
    public bool IsUrgent { get; set; }
    public bool HasReminder { get; set; }
    public string PriorityDisplay { get; set; } = string.Empty;
    public string CategoryDisplay { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TodoListResponse
{
    public List<TodoResponse> Todos { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    
    // Enhanced stats
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public int UrgentCount { get; set; }
}

public class TodoStatsResponse
{
    public int TotalTodos { get; set; }
    public int CompletedTodos { get; set; }
    public int PendingTodos { get; set; }
    public int OverdueTodos { get; set; }
    public int TodaysDueTodos { get; set; }
    public int ThisWeeksDueTodos { get; set; }
    public decimal CompletionRate { get; set; }
    public Dictionary<TodoCategory, int> TodosByCategory { get; set; } = new();
    public Dictionary<TodoPriority, int> TodosByPriority { get; set; } = new();
}
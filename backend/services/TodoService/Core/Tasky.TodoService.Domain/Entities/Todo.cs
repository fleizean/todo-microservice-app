using System.ComponentModel.DataAnnotations;

namespace Tasky.TodoService.Domain.Entities;

public enum TodoPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum TodoCategory
{
    Personal = 1,
    Work = 2,
    Shopping = 3,
    Health = 4,
    Education = 5,
    Finance = 6,
    Other = 7
}

public class Todo
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    // New enhanced fields
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    
    public TodoCategory Category { get; set; } = TodoCategory.Personal;
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? ReminderDateTime { get; set; }
    
    public bool ReminderSent { get; set; } = false;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [MaxLength(50)]
    public string? Tags { get; set; } // Comma-separated tags
    
    public int EstimatedMinutes { get; set; } = 0; // Estimated time to complete
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    // Helper properties
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsCompleted;
    
    public bool IsUrgent => Priority == TodoPriority.Urgent;
    
    public bool HasReminder => ReminderDateTime.HasValue && !ReminderSent;
    
    public string PriorityDisplay => Priority switch
    {
        TodoPriority.Low => "🟢 Low",
        TodoPriority.Medium => "🟡 Medium", 
        TodoPriority.High => "🟠 High",
        TodoPriority.Urgent => "🔴 Urgent",
        _ => "🟡 Medium"
    };
    
    public string CategoryDisplay => Category switch
    {
        TodoCategory.Personal => "👤 Personal",
        TodoCategory.Work => "💼 Work",
        TodoCategory.Shopping => "🛒 Shopping",
        TodoCategory.Health => "🏥 Health",
        TodoCategory.Education => "📚 Education",
        TodoCategory.Finance => "💰 Finance",
        TodoCategory.Other => "📝 Other",
        _ => "📝 Other"
    };
}
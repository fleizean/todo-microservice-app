using System.ComponentModel.DataAnnotations;
using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Application.DTOs.Requests;

public class UpdateTodoRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool? IsCompleted { get; set; }
    
    public TodoPriority? Priority { get; set; }
    
    public TodoCategory? Category { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? ReminderDateTime { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [MaxLength(50)]
    public string? Tags { get; set; }
    
    [Range(0, 1440)]
    public int? EstimatedMinutes { get; set; }
}

public class ToggleTodoRequest
{
    public bool IsCompleted { get; set; }
}
using System.ComponentModel.DataAnnotations;
using Tasky.TodoService.Domain.Entities;

namespace Tasky.TodoService.Application.DTOs.Requests;

public class CreateTodoRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    
    public TodoCategory Category { get; set; } = TodoCategory.Personal;
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? ReminderDateTime { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    [MaxLength(50)]
    public string? Tags { get; set; }
    
    [Range(0, 1440)] // 0 to 24 hours in minutes
    public int EstimatedMinutes { get; set; } = 0;
}
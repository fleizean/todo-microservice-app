namespace Tasky.TodoService.Application.DTOs.Events;

public class TodoCreatedEvent
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodoCompletedEvent
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class TodoDeletedEvent
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
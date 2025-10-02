namespace Tasky.NotificationService.Application.DTOs.Events;

public class TodoCreatedEventDto
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodoCompletedEventDto
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class TodoDeletedEventDto
{
    public int TodoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
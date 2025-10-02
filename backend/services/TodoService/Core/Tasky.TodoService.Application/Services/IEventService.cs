using Tasky.TodoService.Application.DTOs.Events;

namespace Tasky.TodoService.Application.Services;

public interface IEventService
{
    Task PublishTodoCreatedEventAsync(TodoCreatedEvent todoEvent);
    Task PublishTodoCompletedEventAsync(TodoCompletedEvent todoEvent);
    Task PublishTodoDeletedEventAsync(TodoDeletedEvent todoEvent);
}
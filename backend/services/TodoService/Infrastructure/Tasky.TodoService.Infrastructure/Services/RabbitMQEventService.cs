using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Tasky.TodoService.Application.DTOs.Events;
using Tasky.TodoService.Application.Services;

namespace Tasky.TodoService.Infrastructure.Services;

public class RabbitMQEventService : IEventService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "tasky.events";

    public RabbitMQEventService(string connectionString)
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RabbitMQ connection failed: {ex.Message}");
            // Don't throw, let the service continue without RabbitMQ
        }
    }

    public async Task PublishTodoCreatedEventAsync(TodoCreatedEvent todoEvent)
    {
        var message = new
        {
            EventType = "TodoCreated",
            Timestamp = DateTime.UtcNow,
            Data = todoEvent
        };

        await PublishEventAsync("todo.created", message);
    }

    public async Task PublishTodoCompletedEventAsync(TodoCompletedEvent todoEvent)
    {
        var message = new
        {
            EventType = "TodoCompleted",
            Timestamp = DateTime.UtcNow,
            Data = todoEvent
        };

        await PublishEventAsync("todo.completed", message);
    }

    public async Task PublishTodoDeletedEventAsync(TodoDeletedEvent todoEvent)
    {
        var message = new
        {
            EventType = "TodoDeleted",
            Timestamp = DateTime.UtcNow,
            Data = todoEvent
        };

        await PublishEventAsync("todo.deleted", message);
    }

    private async Task PublishEventAsync(string routingKey, object message)
    {
        try
        {
            if (_channel == null || _connection == null || !_connection.IsOpen)
            {
                Console.WriteLine("RabbitMQ is not available, skipping event publish");
                return;
            }

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish event: {ex.Message}");
            // Continue without throwing
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
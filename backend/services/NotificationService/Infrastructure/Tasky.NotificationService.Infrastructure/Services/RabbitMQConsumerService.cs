using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.Services;

public class RabbitMQConsumerService : IMessageConsumerService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly INotificationService _notificationService;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "tasky.events";
    private const string QueueName = "tasky.notifications";

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ__Port") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Exchange ve Queue setup
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
            
            // Multiple routing keys bind
            await _channel.QueueBindAsync(QueueName, ExchangeName, "todo.created");
            await _channel.QueueBindAsync(QueueName, ExchangeName, "todo.completed");
            await _channel.QueueBindAsync(QueueName, ExchangeName, "todo.deleted");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("Received message: {Message}", message);
                    
                    await ProcessMessageAsync(message, ea.RoutingKey);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, false, consumer);
            
            _logger.LogInformation("Notification Service started. Listening to RabbitMQ...");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while connecting to RabbitMQ");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string message, string routingKey)
    {
        try
        {
            var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
            var eventType = messageObj.GetProperty("EventType").GetString();
            var data = messageObj.GetProperty("Data");

            switch (eventType)
            {
                case "TodoCreated":
                    var todoCreatedEvent = JsonSerializer.Deserialize<TodoCreatedEventDto>(data.GetRawText());
                    if (todoCreatedEvent != null)
                    {
                        await _notificationService.CreateNotificationFromTodoCreatedAsync(todoCreatedEvent);
                        _logger.LogInformation("📝 Todo created notification processed for user: {UserId}", todoCreatedEvent.UserId);
                    }
                    break;

                case "TodoCompleted":
                    var todoCompletedEvent = JsonSerializer.Deserialize<TodoCompletedEventDto>(data.GetRawText());
                    if (todoCompletedEvent != null)
                    {
                        await _notificationService.CreateNotificationFromTodoCompletedAsync(todoCompletedEvent);
                        _logger.LogInformation("✅ Todo completed notification processed for user: {UserId}", todoCompletedEvent.UserId);
                    }
                    break;

                case "TodoDeleted":
                    var todoDeletedEvent = JsonSerializer.Deserialize<TodoDeletedEventDto>(data.GetRawText());
                    if (todoDeletedEvent != null)
                    {
                        await _notificationService.CreateNotificationFromTodoDeletedAsync(todoDeletedEvent);
                        _logger.LogInformation("🗑️ Todo deleted notification processed for user: {UserId}", todoDeletedEvent.UserId);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
            throw;
        }
    }

    public async Task StopConsumingAsync()
    {
        _logger.LogInformation("Stopping Notification Service...");
        
        if (_channel != null)
            await _channel.CloseAsync();
            
        if (_connection != null)
            await _connection.CloseAsync();
    }
}
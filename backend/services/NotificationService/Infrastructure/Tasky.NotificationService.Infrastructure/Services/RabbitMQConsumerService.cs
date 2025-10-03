using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.Services;

public class RabbitMQConsumerService : IMessageConsumerService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly INotificationService _notificationService;
    private IConnection? _connection;
    private IModel? _channel;
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
        var maxRetries = 5;
        var delay = TimeSpan.FromSeconds(5);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost",
                    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ__Port") ?? "5672"),
                    UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest",
                    Password = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest",
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("Successfully connected to RabbitMQ.");

                // Exchange ve Queue kurulumu
                _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

                // Routing key'leri bağlama
                _channel.QueueBind(QueueName, ExchangeName, "todo.created");
                _channel.QueueBind(QueueName, ExchangeName, "todo.completed");
                _channel.QueueBind(QueueName, ExchangeName, "todo.deleted");

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("Received message: {Message}", message);

                        await ProcessMessageAsync(message, ea.RoutingKey);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue on failure
                    }
                };

                _channel.BasicConsume(QueueName, false, consumer);

                _logger.LogInformation("Notification Service started. Listening to RabbitMQ...");

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
                return; // Başarılı olursa döngüden çık
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to RabbitMQ. Retry {RetryCount}/{MaxRetries}...", i + 1, maxRetries);
                if (i < maxRetries - 1)
                {
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    _logger.LogError("Could not connect to RabbitMQ after multiple retries. The service will shut down.");
                    throw; // Son denemede de başarısız olursa hatayı fırlat
                }
            }
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

    public Task StopConsumingAsync()
    {
        _logger.LogInformation("Stopping Notification Service...");

        if (_channel != null)
            _channel.Close();

        if (_connection != null)
            _connection.Close();

        return Task.CompletedTask;
    }
}
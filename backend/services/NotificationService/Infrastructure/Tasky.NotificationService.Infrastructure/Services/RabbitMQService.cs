using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Tasky.NotificationService.Application.DTOs.Events;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.Services;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _emailReminderQueueName = "email_reminder_queue";
    private bool _isInitialized = false;
    private readonly object _lock = new object();

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private void EnsureConnection()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration.GetValue<string>("RabbitMQ:HostName") ?? "localhost",
                    Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                    UserName = _configuration.GetValue<string>("RabbitMQ:UserName") ?? "guest",
                    Password = _configuration.GetValue<string>("RabbitMQ:Password") ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                // Declare the email reminder queue
                _channel.QueueDeclare(
                    queue: _emailReminderQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                
                _isInitialized = true;
                _logger.LogInformation("RabbitMQ connection established and queue declared");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to establish RabbitMQ connection - will retry on next operation");
                // Don't throw - let service start without RabbitMQ
            }
        }
    }

    public async Task PublishEmailReminderAsync(string userId, string email, string subject, string body, DateTime scheduledAt)
    {
        EnsureConnection();
        
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ not available - email reminder will not be scheduled");
            return;
        }

        try
        {
            var emailReminder = new EmailReminderDto
            {
                UserId = userId,
                Email = email,
                Subject = subject,
                Body = body,
                ScheduledAt = scheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            var message = JsonConvert.SerializeObject(emailReminder);
            var messageBody = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            
            // Add scheduled timestamp to headers for delay processing
            properties.Headers = new Dictionary<string, object>
            {
                ["scheduled_at"] = scheduledAt.ToBinary()
            };

            _channel.BasicPublish(
                exchange: "",
                routingKey: _emailReminderQueueName,
                basicProperties: properties,
                body: messageBody
            );

            _logger.LogInformation($"Email reminder scheduled for user {userId} at {scheduledAt}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to publish email reminder for user {userId}");
            throw;
        }
    }

    public async Task ConsumeEmailRemindersAsync()
    {
        EnsureConnection();
        
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ not available - email reminder consumer will not start");
            return;
        }

        try
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var emailReminder = JsonConvert.DeserializeObject<EmailReminderDto>(message);

                    if (emailReminder != null)
                    {
                        // Check if it's time to send the email
                        if (DateTime.UtcNow >= emailReminder.ScheduledAt)
                        {
                            await SendEmailAsync(emailReminder);
                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation($"Email reminder sent to {emailReminder.Email}");
                        }
                        else
                        {
                            // Re-queue for later if not yet time
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                            _logger.LogInformation($"Email reminder re-queued for {emailReminder.Email}, scheduled for {emailReminder.ScheduledAt}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email reminder message");
                    _channel.BasicNack(ea.DeliveryTag, false, false); // Don't requeue on error
                }
            };

            _channel.BasicConsume(
                queue: _emailReminderQueueName,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogInformation("Started consuming email reminder messages");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming email reminders");
            throw;
        }
    }

    private async Task SendEmailAsync(EmailReminderDto emailReminder)
    {
        // TODO: Implement actual email sending logic
        // This could integrate with SendGrid, SMTP, or other email services
        _logger.LogInformation($"Sending email to {emailReminder.Email} with subject: {emailReminder.Subject}");
        
        // Simulate email sending
        await Task.Delay(100);
        
        _logger.LogInformation($"Email sent successfully to {emailReminder.Email}");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.BackgroundServices;

public class EmailReminderConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailReminderConsumerService> _logger;

    public EmailReminderConsumerService(
        IServiceProvider serviceProvider,
        ILogger<EmailReminderConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Reminder Consumer Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var rabbitMQService = scope.ServiceProvider.GetRequiredService<IRabbitMQService>();
                
                await rabbitMQService.ConsumeEmailRemindersAsync();
                
                _logger.LogInformation("Email reminder consumer started successfully");
                
                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Email Reminder Consumer Service stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email Reminder Consumer Service encountered an error - will retry in 30 seconds");
                
                try
                {
                    await Task.Delay(30000, stoppingToken); // Wait 30 seconds before retry
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Reminder Consumer Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
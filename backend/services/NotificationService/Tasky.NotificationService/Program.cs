using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Application.Services;
using Tasky.NotificationService.Infrastructure.Repositories;
using Tasky.NotificationService.Infrastructure.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Repositories
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        
        // Application Services
        services.AddScoped<INotificationService, NotificationServiceImpl>();
        services.AddScoped<IMessageConsumerService, RabbitMQConsumerService>();
        
        // Background Service
        services.AddHostedService<NotificationHostedService>();
    })
    .Build();

await host.RunAsync();

public class NotificationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var consumerService = scope.ServiceProvider.GetRequiredService<IMessageConsumerService>();
        
        await consumerService.StartConsumingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var consumerService = scope.ServiceProvider.GetRequiredService<IMessageConsumerService>();
        
        await consumerService.StopConsumingAsync();
        await base.StopAsync(cancellationToken);
    }
}

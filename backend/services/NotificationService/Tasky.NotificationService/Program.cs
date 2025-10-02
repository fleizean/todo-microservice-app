using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Application.Services;
using Tasky.NotificationService.Infrastructure.Data;
using Tasky.NotificationService.Infrastructure.Repositories;
using Tasky.NotificationService.Infrastructure.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Database
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
        
        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        // Application Services
        services.AddScoped<INotificationService, NotificationServiceImpl>();
        services.AddScoped<IMessageConsumerService, RabbitMQConsumerService>();
        
        // Background Service
        services.AddHostedService<NotificationHostedService>();
    })
    .Build();

// Apply pending migrations automatically
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migrations applied successfully for NotificationService.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

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

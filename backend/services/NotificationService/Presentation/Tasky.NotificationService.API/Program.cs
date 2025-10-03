using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Tasky.NotificationService.Application.Repositories;
using Tasky.NotificationService.Application.Services;
using Tasky.NotificationService.Infrastructure.Data;
using Tasky.NotificationService.Infrastructure.Repositories;
using Tasky.NotificationService.Infrastructure.Services;
using Tasky.NotificationService.Infrastructure.BackgroundServices;
using Tasky.NotificationService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add Entity Framework
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories and services
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
builder.Services.AddScoped<IHubNotificationService>(provider => 
    new HubNotificationService(
        provider.GetRequiredService<IHubContext<NotificationHub>>(),
        provider.GetRequiredService<ILogger<HubNotificationService>>()));
builder.Services.AddScoped<IMessageConsumerService, RabbitMQConsumerService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// Add background services
builder.Services.AddHostedService<EmailReminderConsumerService>();
builder.Services.AddHostedService<RabbitMQConsumerHostedService>();

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:80")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

public class RabbitMQConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQConsumerHostedService> _logger;

    public RabbitMQConsumerHostedService(IServiceProvider serviceProvider, ILogger<RabbitMQConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ Consumer Hosted Service starting...");
        
        using var scope = _serviceProvider.CreateScope();
        var consumerService = scope.ServiceProvider.GetRequiredService<IMessageConsumerService>();
        
        await consumerService.StartConsumingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Consumer Hosted Service stopping...");
        
        using var scope = _serviceProvider.CreateScope();
        var consumerService = scope.ServiceProvider.GetRequiredService<IMessageConsumerService>();
        
        await consumerService.StopConsumingAsync();
        await base.StopAsync(cancellationToken);
    }
}
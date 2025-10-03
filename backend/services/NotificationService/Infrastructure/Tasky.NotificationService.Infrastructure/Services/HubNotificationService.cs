using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tasky.NotificationService.Application.DTOs.Responses;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.Infrastructure.Services;

public class HubNotificationService : IHubNotificationService
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly ILogger<HubNotificationService> _logger;

    public HubNotificationService(IHubContext<Hub> hubContext, ILogger<HubNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(string userId, NotificationResponse notification)
    {
        _logger.LogInformation("Sending notification to user {UserId}: {Title}", userId, notification.Title);
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationCountUpdateAsync(string userId, int unreadCount)
    {
        _logger.LogInformation("Sending notification count update to user {UserId}: {Count}", userId, unreadCount);
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("NotificationCountUpdate", unreadCount);
    }
}
namespace Tasky.NotificationService.Application.Services;

public interface IMessageConsumerService
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
    Task StopConsumingAsync();
}
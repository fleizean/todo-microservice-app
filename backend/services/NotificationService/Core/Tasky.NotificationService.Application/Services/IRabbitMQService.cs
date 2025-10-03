namespace Tasky.NotificationService.Application.Services;

public interface IRabbitMQService
{
    Task PublishEmailReminderAsync(string userId, string email, string subject, string body, DateTime scheduledAt);
    Task ConsumeEmailRemindersAsync();
}
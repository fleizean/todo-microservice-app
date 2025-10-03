using Microsoft.AspNetCore.Mvc;
using Tasky.NotificationService.Application.Services;

namespace Tasky.NotificationService.API.Controllers;

[ApiController]
[Route("api/email-reminder")]
public class EmailReminderController : ControllerBase
{
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<EmailReminderController> _logger;

    public EmailReminderController(IRabbitMQService rabbitMQService, ILogger<EmailReminderController> logger)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleEmailReminder([FromBody] ScheduleEmailReminderRequest request)
    {
        try
        {
            await _rabbitMQService.PublishEmailReminderAsync(
                request.UserId, 
                request.Email, 
                request.Subject, 
                request.Body, 
                request.ScheduledAt
            );

            _logger.LogInformation("Email reminder scheduled for user {UserId} at {ScheduledAt}", request.UserId, request.ScheduledAt);
            return Ok(new { message = "Email reminder scheduled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule email reminder for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Failed to schedule email reminder" });
        }
    }
}

public record ScheduleEmailReminderRequest(
    string UserId,
    string Email,
    string Subject,
    string Body,
    DateTime ScheduledAt
);
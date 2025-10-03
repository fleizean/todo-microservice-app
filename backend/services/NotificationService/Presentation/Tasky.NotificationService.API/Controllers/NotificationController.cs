using Microsoft.AspNetCore.Mvc;
using Tasky.NotificationService.Application.Services;
using Tasky.NotificationService.Application.DTOs.Responses;

namespace Tasky.NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isRead = null,
        [FromQuery] int? type = null)
    {
        try
        {
            var notifications = await _notificationService.GetNotificationsAsync(userId, page, pageSize, isRead, type);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving notifications", error = ex.Message });
        }
    }

    [HttpPost("{notificationId}/read")]
    public async Task<ActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking notification as read", error = ex.Message });
        }
    }

    [HttpPost("{userId}/read-all")]
    public async Task<ActionResult> MarkAllAsRead(string userId)
    {
        try
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while marking all notifications as read", error = ex.Message });
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<ActionResult> DeleteNotification(int notificationId)
    {
        try
        {
            await _notificationService.DeleteNotificationAsync(notificationId);
            return Ok(new { message = "Notification deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting notification", error = ex.Message });
        }
    }

    [HttpGet("{userId}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(string userId)
    {
        try
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while getting unread count", error = ex.Message });
        }
    }
}
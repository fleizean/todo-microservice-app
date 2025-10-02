namespace Tasky.AuthService.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}
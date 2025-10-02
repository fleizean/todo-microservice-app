using System.Net;
using System.Net.Mail;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Tasky.AuthService.Application.Abstractions.Services;

namespace Tasky.AuthService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        
        var smtpServer = emailSettings["SmtpServer"];
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var senderEmail = emailSettings["SenderEmail"];
        var senderPassword = emailSettings["SenderPassword"];
        var senderName = emailSettings["SenderName"];
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

        var resetUrl = $"http://localhost:4200/auth/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

        // Load email template from file
        var emailBody = await LoadEmailTemplateAsync("PasswordResetEmail.html", new Dictionary<string, string>
        {
            { "{{RESET_URL}}", resetUrl },
            { "{{EXPIRY_TIME}}", "1 hour" },
            { "{{APP_NAME}}", "Tasky" }
        });

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail!, senderName),
            Subject = "🔐 Reset Your Tasky Password",
            Body = emailBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        using var smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = enableSsl
        };

        await smtpClient.SendMailAsync(mailMessage);
    }

    private async Task<string> LoadEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Tasky.AuthService.Infrastructure.Templates.{templateName}";
        
        // First try to read from embedded resource
        using var stream = assembly.GetManifestResourceStream(resourceName);
        string templateContent;
        
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            templateContent = await reader.ReadToEndAsync();
        }
        else
        {
            // Fallback: read from file system
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateName);
            if (!File.Exists(templatePath))
            {
                // Try relative path from assembly location
                var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                templatePath = Path.Combine(assemblyLocation!, "..", "..", "..", "..", "Infrastructure", "Tasky.AuthService.Infrastructure", "Templates", templateName);
            }
            
            if (File.Exists(templatePath))
            {
                templateContent = await File.ReadAllTextAsync(templatePath);
            }
            else
            {
                throw new FileNotFoundException($"Email template '{templateName}' not found.");
            }
        }

        // Replace placeholders
        foreach (var placeholder in placeholders)
        {
            templateContent = templateContent.Replace(placeholder.Key, placeholder.Value);
        }

        return templateContent;
    }
}
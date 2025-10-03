using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tasky.NotificationService.Infrastructure.Data;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=TaskyNotificationDb;User Id=sa;Password=reallyStrongPassword1!;Encrypt=False;TrustServerCertificate=True");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
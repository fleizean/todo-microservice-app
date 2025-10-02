using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tasky.AuthService.Persistence.Contexts;

namespace Tasky.AuthService.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=TaskyAuthDb;User Id=sa;Password=reallyStrongPassword1!;Encrypt=False;TrustServerCertificate=True");

        return new AuthDbContext(optionsBuilder.Options);
    }
}
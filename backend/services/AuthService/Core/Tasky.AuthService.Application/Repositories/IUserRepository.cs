using Tasky.AuthService.Domain.Entities;

namespace Tasky.AuthService.Application.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<AppUser> CreateAsync(AppUser user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task UpdateAsync(AppUser user);
    Task<AppUser?> GetByPasswordResetTokenAsync(string token);
}
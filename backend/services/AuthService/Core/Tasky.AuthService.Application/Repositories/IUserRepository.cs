using Tasky.AuthService.Domain.Entities;

namespace Tasky.AuthService.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task UpdateAsync(User user);
    Task<User?> GetByPasswordResetTokenAsync(string token);
}
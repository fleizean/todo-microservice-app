using Tasky.AuthService.Application.DTOs.Requests;
using Tasky.AuthService.Application.DTOs.Responses;

namespace Tasky.AuthService.Application.Abstractions.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> UpdateUserAvatarAsync(string email, string avatarUrl);
}
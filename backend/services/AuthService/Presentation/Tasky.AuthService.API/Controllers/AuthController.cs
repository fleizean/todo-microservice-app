using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasky.AuthService.Application.Abstractions.Services;
using Tasky.AuthService.Application.DTOs.Requests;
using Tasky.AuthService.Infrastructure.Services;

namespace Tasky.AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly ICloudinaryService _cloudinaryService;

    public AuthController(
        IAuthService authService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator,
        ICloudinaryService cloudinaryService)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var result = await _authService.RegisterAsync(request);
            if (result)
            {
                return Ok(new { message = "User registered successfully" });
            }

            return BadRequest(new { message = "Registration failed" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            await _authService.ForgotPasswordAsync(request);
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { message = "Email, token, and new password are required" });
            }

            await _authService.ResetPasswordAsync(request);
            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while resetting password" });
        }
    }

       [HttpPost("upload-avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }
    
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Only image files (JPEG, PNG, GIF) are allowed" });
            }
    
            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size cannot exceed 5MB" });
            }
    
            // Get user email from JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
    
            // Upload to Cloudinary
            string? avatarUrl;
            using (var stream = file.OpenReadStream())
            {
                avatarUrl = await _cloudinaryService.UploadImageAsync(stream, file.FileName);
            }
    
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return StatusCode(500, new { message = "Failed to upload image" });
            }
    
            // Update user avatar URL in database
            var updated = await _authService.UpdateUserAvatarAsync(userEmail, avatarUrl);
            if (!updated)
            {
                return StatusCode(500, new { message = "Failed to update user avatar" });
            }
    
            return Ok(new { message = "Avatar uploaded successfully", avatarUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while uploading avatar: {ex.Message}" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        try
        {
            // Check for service-to-service authentication
            var serviceKey = Request.Headers["X-Service-Key"].FirstOrDefault();
            var expectedServiceKey = "tasky-service-internal-key"; // In production, get this from configuration
            
            if (string.IsNullOrEmpty(serviceKey) || serviceKey != expectedServiceKey)
            {
                return Unauthorized(new { message = "Invalid service authentication" });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }
}
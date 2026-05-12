namespace domain.interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string? Error, string? IdentityId)> CreateUserAsync(string email, string password);
    Task<(bool Success, string? IdentityId, string? Email)> CheckPasswordAsync(string email, string password);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
}
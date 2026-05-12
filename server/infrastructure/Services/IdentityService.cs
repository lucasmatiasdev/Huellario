using domain.interfaces;
using infrastructure.identity;
using Microsoft.AspNetCore.Identity;

namespace infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<HuellarioIdentityUser> _userManager;

    public IdentityService(UserManager<HuellarioIdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? Error, string? IdentityId)> CreateUserAsync(string email, string password)
    {
        var identityUser = new HuellarioIdentityUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(identityUser, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Error al crear usuario: {errors}", null);
        }

        return (true, null, identityUser.Id);
    }

    public async Task<(bool Success, string? IdentityId, string? Email)> CheckPasswordAsync(string email, string password)
    {
        var identityUser = await _userManager.FindByEmailAsync(email);
        if (identityUser == null)
            return (false, null, null);

        var validPassword = await _userManager.CheckPasswordAsync(identityUser, password);
        if (!validPassword)
            return (false, null, null);

        return (true, identityUser.Id, identityUser.Email);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using application.interfaces;
using application.dtos.Auth;
using domain.entities;
using domain.interfaces;
using Mapster;
using Microsoft.IdentityModel.Tokens;

namespace application.implementations;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var (succeeded, error, identityId) = await _identityService.CreateUserAsync(registerDto.Email, registerDto.Password);
            if (!succeeded)
                throw new InvalidOperationException(error);

            var user = registerDto.Adapt<User>();
            user.IdentityId = identityId;
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();
            return GenerateTokenResponse(identityId!, registerDto.Email, user);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
    {
        var (success, identityId, email) = await _identityService.CheckPasswordAsync(loginDto.Email, loginDto.Password);
        if (!success || identityId == null)
            throw new UnauthorizedAccessException("Credenciales inv\u00e1lidas");

        var domainUser = await _unitOfWork.Users.GetByIdentityIdAsync(identityId);
        if (domainUser == null)
            throw new UnauthorizedAccessException("Usuario no encontrado");

        return GenerateTokenResponse(identityId, email ?? loginDto.Email, domainUser);
    }

    public Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException("Refresh token no implementado");
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var token = await _identityService.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);
        if (token == null)
            return true;

        // TODO: Enviar token por email
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        return await _identityService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.Token, resetPasswordDto.NewPassword);
    }

    private TokenResponseDto GenerateTokenResponse(string identityId, string email, User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identityId),
            new Claim(ClaimTypes.Email, email),
            new Claim("UserId", user.Id.ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new TokenResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            Email = email,
            Name = $"{user.Name} {user.Surname}"
        };
    }
}
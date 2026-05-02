using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using application.interfaces;
using domain.dtos.Auth;
using domain.entities;
using domain.interfaces;
using infrastructure.identity;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace server.services;

public class AuthService : IAuthService
{
    private readonly UserManager<HuellarioIdentityUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<HuellarioIdentityUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var user = registerDto.Adapt<User>();

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var identityUser = new HuellarioIdentityUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            UserId = user.Id
        };

        var result = await _userManager.CreateAsync(identityUser, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error al crear usuario: {errors}");
        }

        user.IdentityId = identityUser.Id;
        await _unitOfWork.SaveChangesAsync();

        return GenerateTokenResponse(identityUser, user);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
    {
        var identityUser = await _userManager.FindByEmailAsync(loginDto.Email);
        if (identityUser == null)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var validPassword = await _userManager.CheckPasswordAsync(identityUser, loginDto.Password);
        if (!validPassword)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var domainUser = await _unitOfWork.Users.GetByIdentityIdAsync(identityUser.Id);
        if (domainUser == null)
            throw new UnauthorizedAccessException("Usuario no encontrado");

        return GenerateTokenResponse(identityUser, domainUser);
    }

    public Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException("Refresh token no implementado");
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
            return true;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // TODO: Enviar token por email
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        return result.Succeeded;
    }

    private TokenResponseDto GenerateTokenResponse(HuellarioIdentityUser identityUser, User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
            new Claim(ClaimTypes.Email, identityUser.Email ?? ""),
            new Claim("UserId", user.Id.ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new TokenResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            Email = identityUser.Email ?? "",
            Name = $"{user.Name} {user.Surname}"
        };
    }
}

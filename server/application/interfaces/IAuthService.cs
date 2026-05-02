using domain.dtos.Auth;

namespace application.interfaces;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<TokenResponseDto> LoginAsync(LoginDto loginDto);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
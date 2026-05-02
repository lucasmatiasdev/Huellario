using application.interfaces;
using domain.dtos.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace server.controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
    private readonly IValidator<RefreshTokenDto> _refreshTokenValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ForgotPasswordDto> forgotPasswordValidator,
        IValidator<ResetPasswordDto> resetPasswordValidator,
        IValidator<RefreshTokenDto> refreshTokenValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _refreshTokenValidator = refreshTokenValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register(RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshTokenDto refreshToken)
    {
        var validation = await _refreshTokenValidator.ValidateAsync(refreshToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshToken.Token);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Refresh token no implementado" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        await _authService.ForgotPasswordAsync(dto);
        return Ok(new { message = "Si el email está registrado, recibirás un enlace para resetear tu contraseña" });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _authService.ResetPasswordAsync(dto);
        if (!result)
            return BadRequest(new { error = "Token inválido o expirado" });

        return Ok(new { message = "Contraseña actualizada correctamente" });
    }
}

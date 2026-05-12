using application.implementations;
using application.dtos.Auth;
using domain.entities;
using domain.interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using application;

namespace tests;

public class AuthServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _jwtSettings = Options.Create(new JwtSettings
        {
            Key = "supersecretkeythatshouldbelongenoughforhmac256",
            Issuer = "Huellario",
            Audience = "Huellario",
            ExpirationMinutes = 60
        });

        _sut = new AuthService(_identityServiceMock.Object, _unitOfWorkMock.Object, _jwtSettings);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnTokenResponseDto_WhenSucceeds()
    {
        var dto = new RegisterDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "juan@test.com",
            Password = "Pass123!"
        };

        _identityServiceMock
            .Setup(i => i.CreateUserAsync(dto.Email, dto.Password))
            .ReturnsAsync((true, null, "identity-id-1"));

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 1);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(default)).Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(dto);

        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Email.ShouldBe("juan@test.com");
        result.Name.ShouldBe("Juan Perez");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenIdentityCreationFails()
    {
        var dto = new RegisterDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "juan@test.com",
            Password = "weak"
        };

        _identityServiceMock
            .Setup(i => i.CreateUserAsync(dto.Email, dto.Password))
            .ReturnsAsync((false, "Error al crear usuario: Password too weak", (string?)null));

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);

        var act = () => _sut.RegisterAsync(dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("Password too weak");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenEmailNotFound()
    {
        var dto = new LoginDto { Email = "noexiste@test.com", Password = "Pass123!" };

        _identityServiceMock
            .Setup(i => i.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync((false, null, null));

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordInvalid()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "wrong" };

        _identityServiceMock
            .Setup(i => i.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync((false, null, null));

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenDomainUserNotFound()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "Pass123!" };

        _identityServiceMock
            .Setup(i => i.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync((true, "id-1", "juan@test.com"));

        _userRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync("id-1"))
            .ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenResponseDto_WhenCredentialsValid()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "Pass123!" };
        var domainUser = new User { Id = 1, IdentityId = "id-1", Name = "Juan", Surname = "Perez" };

        _identityServiceMock
            .Setup(i => i.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync((true, "id-1", "juan@test.com"));

        _userRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync("id-1"))
            .ReturnsAsync(domainUser);

        var result = await _sut.LoginAsync(dto);

        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Email.ShouldBe("juan@test.com");
        result.Name.ShouldBe("Juan Perez");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        var dto = new ResetPasswordDto { Email = "noexiste@test.com", Token = "token", NewPassword = "NewPass123!" };

        _identityServiceMock
            .Setup(i => i.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword))
            .ReturnsAsync(false);

        var result = await _sut.ResetPasswordAsync(dto);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnTrue_WhenResetSucceeds()
    {
        var dto = new ResetPasswordDto { Email = "juan@test.com", Token = "valid-token", NewPassword = "NewPass123!" };

        _identityServiceMock
            .Setup(i => i.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword))
            .ReturnsAsync(true);

        var result = await _sut.ResetPasswordAsync(dto);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrue_WhenUserNotFound()
    {
        var dto = new ForgotPasswordDto { Email = "noexiste@test.com" };

        _identityServiceMock
            .Setup(i => i.GeneratePasswordResetTokenAsync(dto.Email))
            .ReturnsAsync((string?)null);

        var result = await _sut.ForgotPasswordAsync(dto);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldReturnTrue_WhenTokenGenerated()
    {
        var dto = new ForgotPasswordDto { Email = "juan@test.com" };

        _identityServiceMock
            .Setup(i => i.GeneratePasswordResetTokenAsync(dto.Email))
            .ReturnsAsync("reset-token");

        var result = await _sut.ForgotPasswordAsync(dto);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowNotImplementedException()
    {
        var act = () => _sut.RefreshTokenAsync("some-token");

        await act.ShouldThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task RegisterAsync_ShouldRollbackTransaction_WhenDomainSaveFails()
    {
        var dto = new RegisterDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "juan@test.com",
            Password = "Pass123!"
        };

        _identityServiceMock
            .Setup(i => i.CreateUserAsync(dto.Email, dto.Password))
            .ReturnsAsync((true, null, "identity-id-1"));

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 1);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ThrowsAsync(new Exception("DB error"));

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);

        var act = () => _sut.RegisterAsync(dto);

        await act.ShouldThrowAsync<Exception>();
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
    }
}
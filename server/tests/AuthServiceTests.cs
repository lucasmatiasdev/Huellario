using domain.dtos.Auth;
using domain.entities;
using domain.interfaces;
using Huellario.Server.Services;
using infrastructure.identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;

namespace tests;

public class AuthServiceTests
{
    private readonly Mock<IUserStore<HuellarioIdentityUser>> _userStoreMock;
    private readonly Mock<UserManager<HuellarioIdentityUser>> _userManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userStoreMock = new Mock<IUserStore<HuellarioIdentityUser>>();
        _userManagerMock = new Mock<UserManager<HuellarioIdentityUser>>(
            _userStoreMock.Object, null, null, null, null, null, null, null, null
        );

        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        //Estos no son mis credenciales posta
        var configData = new Dictionary<string, string>
        {
            { "Jwt:Key", "supersecretkeythatshouldbelongenoughforhmac256" },
            { "Jwt:Issuer", "Huellario" },
            { "Jwt:Audience", "Huellario" },
            { "Jwt:ExpirationMinutes", "60" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new AuthService(_userManagerMock.Object, _unitOfWorkMock.Object, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnTokenResponseDto_WhenSucceeds()
    {
        var dto = new RegisterDto
        {
            Name = "Juan",
            Surname = "Pérez",
            Email = "juan@test.com",
            Password = "Pass123!"
        };

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 1);

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<HuellarioIdentityUser>(), It.IsAny<string>()))
            .Callback<HuellarioIdentityUser, string>((user, _) => user.Id = "identity-id-1")
            .ReturnsAsync(IdentityResult.Success);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.RegisterAsync(dto);

        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Email.ShouldBe("juan@test.com");
        result.Name.ShouldBe("Juan Pérez");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Exactly(2));
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<HuellarioIdentityUser>(), dto.Password), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenUserManagerFails()
    {
        var dto = new RegisterDto
        {
            Name = "Juan",
            Surname = "Pérez",
            Email = "juan@test.com",
            Password = "weak"
        };

        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 1);

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<HuellarioIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var act = () => _sut.RegisterAsync(dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("Password too weak");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenEmailNotFound()
    {
        var dto = new LoginDto { Email = "noexiste@test.com", Password = "Pass123!" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync((HuellarioIdentityUser?)null);

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenPasswordInvalid()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "wrong" };
        var identityUser = new HuellarioIdentityUser { Id = "id-1", UserName = "juan@test.com", Email = "juan@test.com" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(identityUser);

        _userManagerMock
            .Setup(um => um.CheckPasswordAsync(identityUser, dto.Password))
            .ReturnsAsync(false);

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenDomainUserNotFound()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "Pass123!" };
        var identityUser = new HuellarioIdentityUser { Id = "id-1", UserName = "juan@test.com", Email = "juan@test.com" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(identityUser);

        _userManagerMock
            .Setup(um => um.CheckPasswordAsync(identityUser, dto.Password))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync(identityUser.Id))
            .ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(dto);

        await act.ShouldThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenResponseDto_WhenCredentialsValid()
    {
        var dto = new LoginDto { Email = "juan@test.com", Password = "Pass123!" };
        var identityUser = new HuellarioIdentityUser { Id = "id-1", UserName = "juan@test.com", Email = "juan@test.com" };
        var domainUser = new User { Id = 1, IdentityId = "id-1", Name = "Juan", Surname = "Pérez" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(identityUser);

        _userManagerMock
            .Setup(um => um.CheckPasswordAsync(identityUser, dto.Password))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(r => r.GetByIdentityIdAsync(identityUser.Id))
            .ReturnsAsync(domainUser);

        var result = await _sut.LoginAsync(dto);

        result.ShouldNotBeNull();
        result.Token.ShouldNotBeNullOrEmpty();
        result.Email.ShouldBe("juan@test.com");
        result.Name.ShouldBe("Juan Pérez");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        var dto = new ResetPasswordDto { Email = "noexiste@test.com", Token = "token", NewPassword = "NewPass123!" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync((HuellarioIdentityUser?)null);

        var result = await _sut.ResetPasswordAsync(dto);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnTrue_WhenResetSucceeds()
    {
        var dto = new ResetPasswordDto { Email = "juan@test.com", Token = "valid-token", NewPassword = "NewPass123!" };
        var identityUser = new HuellarioIdentityUser { Id = "id-1", Email = "juan@test.com" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(identityUser);

        _userManagerMock
            .Setup(um => um.ResetPasswordAsync(identityUser, dto.Token, dto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.ResetPasswordAsync(dto);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFalse_WhenResetFails()
    {
        var dto = new ResetPasswordDto { Email = "juan@test.com", Token = "invalid-token", NewPassword = "NewPass123!" };
        var identityUser = new HuellarioIdentityUser { Id = "id-1", Email = "juan@test.com" };

        _userManagerMock
            .Setup(um => um.FindByEmailAsync(dto.Email))
            .ReturnsAsync(identityUser);

        _userManagerMock
            .Setup(um => um.ResetPasswordAsync(identityUser, dto.Token, dto.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _sut.ResetPasswordAsync(dto);

        result.ShouldBeFalse();
    }
}

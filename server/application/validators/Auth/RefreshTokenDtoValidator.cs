using domain.dtos.Auth;
using FluentValidation;

namespace application.validators.Auth;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
    }
}

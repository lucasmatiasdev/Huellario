using domain.dtos.Brand;
using FluentValidation;

namespace application.validators.Brand;

public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
{
    public CreateBrandDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => x.LogoUrl is not null);
    }
}

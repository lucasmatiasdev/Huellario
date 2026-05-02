using domain.dtos.Product;
using FluentValidation;

namespace application.validators.Product;

public class CreateVariantDtoValidator : AbstractValidator<CreateVariantDto>
{
    public CreateVariantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}

using application.dtos.Product;
using FluentValidation;

namespace application.validators.Product;

public class CreateVariantDtoValidator : AbstractValidator<CreateVariantDto>
{
    public CreateVariantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .MaximumLength(50)
            .When(x => x.Sku is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}

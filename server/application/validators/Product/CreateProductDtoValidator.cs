using domain.dtos.Product;
using FluentValidation;

namespace application.validators.Product;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.BrandId)
            .GreaterThan(0);
    }
}

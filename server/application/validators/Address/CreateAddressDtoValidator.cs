using domain.dtos.Address;
using FluentValidation;

namespace application.validators.Address;

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Complement)
            .MaximumLength(200)
            .When(x => x.Complement is not null);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Province)
            .MaximumLength(100)
            .When(x => x.Province is not null);

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Reference)
            .MaximumLength(500)
            .When(x => x.Reference is not null);

        RuleFor(x => x.Type)
            .IsInEnum();
    }
}

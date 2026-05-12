using application.dtos.Order;
using FluentValidation;

namespace application.validators.Order;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.AddressId)
            .GreaterThan(0)
            .When(x => !x.IsPickup);

        RuleFor(x => x.Note)
            .MaximumLength(2000)
            .When(x => x.Note is not null);
    }
}

using domain.dtos.Order;
using FluentValidation;

namespace application.validators.Order;

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}

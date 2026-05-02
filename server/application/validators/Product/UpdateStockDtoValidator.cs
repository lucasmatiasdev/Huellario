using domain.dtos.Product;
using FluentValidation;

namespace application.validators.Product;

public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockDtoValidator()
    {
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);
    }
}

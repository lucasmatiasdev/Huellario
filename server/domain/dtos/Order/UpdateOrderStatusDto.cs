using domain.enums;

namespace domain.dtos.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}

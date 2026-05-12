using domain.enums;

namespace application.dtos.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}

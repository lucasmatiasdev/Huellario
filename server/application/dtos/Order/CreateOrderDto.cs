namespace application.dtos.Order;

public class CreateOrderDto
{
    public int? AddressId { get; set; }
    public bool IsPickup { get; set; }
    public string? Note { get; set; }
}

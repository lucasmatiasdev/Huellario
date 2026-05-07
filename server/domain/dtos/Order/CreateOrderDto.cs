namespace domain.dtos.Order;

public class CreateOrderDto
{
    public int? AddressId { get; set; }
    public bool IsRetirement { get; set; }
    public string? Note { get; set; }
}

using domain.enums;

namespace application.dtos.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int? AddressId { get; set; }
    public string? AddressLine { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsPickup { get; set; }
    public string? Note { get; set; }
    public string? TrackingNumber { get; set; }
    public string? SessionId { get; set; }
    public List<OrderLineDto> OrderLines { get; set; } = new();
}

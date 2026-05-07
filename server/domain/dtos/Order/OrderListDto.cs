using domain.enums;

namespace domain.dtos.Order;

public class OrderListDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsRetirement { get; set; }
    public int ItemCount { get; set; }
    public string? UserName { get; set; }
    public string? Note { get; set; }
}

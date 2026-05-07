using domain.enums;

namespace domain.entities;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? AddressId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsRetirement { get; set; }
    public string? Note { get; set; }
    public string? TrackingNumber { get; set; }

    public User? User { get; set; }
    public Address? Address { get; set; }
    public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
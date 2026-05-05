namespace domain.entities;

public class CartItem
{
    public int UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int VariantId { get; set; }
    public int Quantity { get; set; }

    public Product? Product { get; set; }
    public Variant? Variant { get; set; }
}
namespace domain.dtos.CartItem;

public class CartItemDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string ProductSlug { get; set; }
    public string? ProductImageUrl { get; set; }
    public int VariantId { get; set; }
    public required string VariantName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

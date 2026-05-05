namespace domain.dtos.CartItem;

public class CreateCartItemDto
{
    public string? SessionId { get; set; }
    public int ProductId { get; set; }
    public int VariantId { get; set; }
    public int Quantity { get; set; }
}

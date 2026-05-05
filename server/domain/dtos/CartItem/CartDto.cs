namespace domain.dtos.CartItem;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItems => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.Subtotal);
}

namespace domain.dtos.Product;

public class VariantDto{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
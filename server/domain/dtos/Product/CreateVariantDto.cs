namespace domain.dtos.Product;

public class CreateVariantDto{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

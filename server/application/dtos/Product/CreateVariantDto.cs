namespace application.dtos.Product;

public class CreateVariantDto{
    public required string Name { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; }
}

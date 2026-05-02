namespace domain.entities;

public class Variant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }

    public Product? Product { get; set; }
}

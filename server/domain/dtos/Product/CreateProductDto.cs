namespace domain.dtos.Product;

public class CreateProductDto{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool? IsOwnBrand { get; set; }
    public bool? IsActive { get; set; }
}
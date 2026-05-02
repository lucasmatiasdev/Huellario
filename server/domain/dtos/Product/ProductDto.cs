namespace domain.dtos.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsOwnBrand { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<VariantDto> Variants { get; set; } = new();
}
namespace domain.entities;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsOwnBrand { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Variant> Variants { get; set; } = new List<Variant>();
}

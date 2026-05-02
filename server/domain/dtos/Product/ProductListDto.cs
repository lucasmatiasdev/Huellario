namespace domain.dtos.Product;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public bool IsOwnBrand { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MainImageUrl { get; set; }
}
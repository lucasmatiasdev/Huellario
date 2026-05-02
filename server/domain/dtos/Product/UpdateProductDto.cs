namespace domain.dtos.Product;

public class UpdateProductDto{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public bool? IsOwnBrand { get; set; }
    public bool? IsActive { get; set; }
}
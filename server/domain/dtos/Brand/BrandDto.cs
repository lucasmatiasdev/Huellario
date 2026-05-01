namespace domain.dtos.Brand;

public class BrandDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}
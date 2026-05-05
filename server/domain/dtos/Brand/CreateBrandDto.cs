namespace domain.dtos.Brand;

public class CreateBrandDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
}
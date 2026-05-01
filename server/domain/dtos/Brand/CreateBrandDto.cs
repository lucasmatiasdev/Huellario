namespace domain.dtos.Brand;

public class CreateBrandDto
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
}
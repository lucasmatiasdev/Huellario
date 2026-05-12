namespace application.dtos.Brand;

public class UpdateBrandDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
}

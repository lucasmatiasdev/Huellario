namespace domain.dtos.Category;

public class CreateCategoryDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImagenUrl { get; set; }
    public bool IsActive { get; set; }   
}
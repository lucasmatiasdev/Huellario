namespace application.dtos.Category;

public class UpdateCategoryDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? ImagenUrl { get; set; }
    public bool? IsActive { get; set; }   
}

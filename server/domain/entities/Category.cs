namespace domain.entities;

public class Category
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImagenUrl { get; set; }
    public bool IsActive { get; set; }
}

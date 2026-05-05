namespace domain.entities;

public class User
{
    public int Id { get; set; }
    public string? IdentityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime RegisterDate { get; set; }
    
}
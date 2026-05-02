namespace domain.entities;

public class User
{
    public int Id { get; set; }
    public string? IdentityId { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Phone { get; set; }
    public DateTime RegisterDate { get; set; }
    
}
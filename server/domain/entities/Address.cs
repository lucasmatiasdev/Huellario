using domain.enums;

namespace domain.entities;
public class Address{
    public int Id { get; set; }   
    public int? UserId { get; set; }
    public User? User { get; set; }
    public required string Street { get; set; }     
    public required string Number { get; set; }     
    public string? Complement { get; set; }
    public required string City { get; set; }      
    public string? Province { get; set; }
    public required string ZipCode { get; set; }    
    public string? Reference { get; set; }   
    public bool IsDefault { get; set; }   
    public AddressType Type { get; set; } 
}
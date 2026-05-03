using domain.enums;

namespace domain.dtos.Address;

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Province { get; set; }
    public string ZipCode { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public bool IsDefault { get; set; }
    public AddressType Type { get; set; }
}

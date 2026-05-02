using domain.entities;
using Microsoft.AspNetCore.Identity;

namespace infrastructure.identity;

public class HuellarioIdentityUser : IdentityUser
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

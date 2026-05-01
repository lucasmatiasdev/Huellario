using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Huellario.Infrastructure.Identity;

namespace Huellario.Infrastructure.Data;

public class HuellarioDbContext : IdentityDbContext<HuellarioIdentityUser>
{
    public HuellarioDbContext(DbContextOptions<HuellarioDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}

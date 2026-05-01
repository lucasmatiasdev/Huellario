using domain.entities;
using infrastructure.data.configurations;
using infrastructure.identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.data;

public class HuellarioDbContext : IdentityDbContext<HuellarioIdentityUser>
{
    public HuellarioDbContext(DbContextOptions<HuellarioDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new CategoryConfiguration());
    }

    public DbSet<Category> Categories => Set<Category>();
}
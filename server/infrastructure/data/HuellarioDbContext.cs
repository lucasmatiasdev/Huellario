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
        builder.ApplyConfiguration(new BrandConfiguration());
        builder.ApplyConfiguration(new UserConfiguration());
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public new DbSet<User> Users => Set<User>();
}
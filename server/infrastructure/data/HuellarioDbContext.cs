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
        builder.ApplyConfiguration(new ProductConfiguration());
        builder.ApplyConfiguration(new ProductImageConfiguration());
        builder.ApplyConfiguration(new VariantConfiguration());
        builder.ApplyConfiguration(new AddressConfiguration());
        builder.ApplyConfiguration(new CartItemConfiguration());
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public new DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Address> Addresses => Set<Address>();   
    public DbSet<CartItem> CartItems => Set<CartItem>();
}
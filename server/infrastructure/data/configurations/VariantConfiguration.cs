using domain.entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.data.configurations;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable("Variants");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Sku)
            .HasMaxLength(50);

        builder.Property(v => v.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(v => v.Stock)
            .HasDefaultValue(0);

        builder.Property(v => v.IsActive)
            .HasDefaultValue(true);
    }
}
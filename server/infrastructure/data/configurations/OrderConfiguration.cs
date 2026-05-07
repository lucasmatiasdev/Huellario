using domain.entities;
using domain.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.data.configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderDate)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(o => o.Total)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasDefaultValue(OrderStatus.Pending)
            .HasConversion<int>();

        builder.Property(o => o.IsRetirement)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.Note)
            .HasMaxLength(2000);

        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Address)
            .WithMany()
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderLines)
            .WithOne(ol => ol.Order)
            .HasForeignKey(ol => ol.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.HasKey(ol => ol.Id);

        builder.Property(ol => ol.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(ol => ol.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasOne(ol => ol.Order)
            .WithMany(o => o.OrderLines)
            .HasForeignKey(ol => ol.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ol => ol.Product)
            .WithMany()
            .HasForeignKey(ol => ol.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ol => ol.Variant)
            .WithMany()
            .HasForeignKey(ol => ol.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

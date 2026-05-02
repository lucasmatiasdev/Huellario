using domain.entities;
using infrastructure.identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.data.configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.IdentityId)
            .HasMaxLength(450);

        builder.HasIndex(u => u.IdentityId)
            .IsUnique()
            .HasFilter("\"IdentityId\" IS NOT NULL");

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Surname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.RegisterDate)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasOne<HuellarioIdentityUser>()
            .WithOne(i => i.User)
            .HasForeignKey<HuellarioIdentityUser>(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

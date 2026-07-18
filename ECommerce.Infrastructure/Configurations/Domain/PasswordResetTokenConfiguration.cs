using ECommerce.Domain.Entities.PasswordReset;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{    
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("ECOMMERCE_PasswordResetToken");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(44);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UsedAt);
        builder.Property(x => x.Used).HasDefaultValue(false);
        builder.Property(x => x.CreatedByIp).HasMaxLength(45);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
    }     
}

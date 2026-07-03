using ECommerce.Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class PendingTwoFactorLoginConfiguration : IEntityTypeConfiguration<PendingTwoFactorLogin>
{
    public void Configure(EntityTypeBuilder<PendingTwoFactorLogin> builder)
    {
        builder.ToTable("PendingTwoFactorLogins");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PendingTwoFactorToken).HasMaxLength(200);
        builder.Property(x => x.PendingTokenExpiresAt);
        builder.Property(x => x.IsUsed).HasDefaultValue(false);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PendingTwoFactorToken).IsUnique(false);
    }
}

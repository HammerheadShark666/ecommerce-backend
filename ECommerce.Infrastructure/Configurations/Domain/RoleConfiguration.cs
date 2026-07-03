using ECommerce.Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations.Domain;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(200);

        builder.HasMany(r => r.UserRoles)
               .WithOne(ur => ur.Role)
               .HasForeignKey(ur => ur.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

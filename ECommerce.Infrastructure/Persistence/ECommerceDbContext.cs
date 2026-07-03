using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> o) : DbContext(o){
    protected override void OnModelCreating(ModelBuilder b) => b.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
}

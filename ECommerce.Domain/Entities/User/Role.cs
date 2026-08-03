using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.User;

public class Role : AuditableEntity<int>
{ 
    public string Name { get; set; } = null!; // e.g. Admin, User, Manager
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}

using ECommerce.Domain.Common;
using ECommerce.Domain.Entities.User;

namespace ECommerce.Domain.Entities.Authentication;

public class Role : AuditableEntity
{
    public string Name { get; set; } = null!; // e.g. Admin, User, Manager
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}

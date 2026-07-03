using ECommerce.Domain.Common;
using ECommerce.Domain.Entities.Authentication;

namespace ECommerce.Domain.Entities.User;

public class UserRole : AuditableEntity
{
	public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

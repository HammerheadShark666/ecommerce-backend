using ECommerce.Domain.Common;
using ECommerce.Domain.Entities.Authentication;

namespace ECommerce.Domain.Entities.User;

public class UserRole
{
	public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

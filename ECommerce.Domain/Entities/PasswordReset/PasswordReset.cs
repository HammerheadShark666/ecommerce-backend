using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.PasswordReset;

public class PasswordResetToken: AuditableEntity<Guid>
{ 
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; } 
    public bool Used { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string? CreatedByIp { get; set; }

    public Domain.Entities.User.User User { get; set; } = null!;
}

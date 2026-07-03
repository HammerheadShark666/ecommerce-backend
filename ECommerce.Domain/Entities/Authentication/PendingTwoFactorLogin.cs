namespace ECommerce.Domain.Entities.Authentication;

public class PendingTwoFactorLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }   
    public string? PendingTwoFactorToken { get; set; }
    public DateTimeOffset? PendingTokenExpiresAt { get; set; }
    public bool IsUsed { get; set; }

    public User.User User { get; set; } = null!;
}

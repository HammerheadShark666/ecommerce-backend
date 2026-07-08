using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities.Product;

public class Review : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public byte Rating { get; set; }

    public string? Title { get; set; }

    public string? Comment { get; set; }

    public bool IsApproved { get; set; }

    public User.User User { get; set; } = null!;

    public Product Product { get; set; } = null!;
}

using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.User;

public class UserAddress : AuditableEntity<Guid>
{
    public Guid UserId { get; private set; }

    public User User { get; private set; } = default!;

    public Address Address { get; private set; } = default!;

    public bool IsPrimary { get; private set; }

    private UserAddress() { }

    public UserAddress(Guid userId, Address address, bool isPrimary = false)
    {
        UserId = userId;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        IsPrimary = isPrimary;
    } 

    public void UpdateAddress(Address address) => Address = address;

    public void SetPrimary() => IsPrimary = true;

    public void RemovePrimary() => IsPrimary = false;
}


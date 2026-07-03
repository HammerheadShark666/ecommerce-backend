using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.User;

public class UserAddress : AuditableEntity
{
    public Guid UserId { get; private set; }

    public Address Address { get; private set; } = default!;

    public bool IsPrimary { get; private set; }

    private UserAddress() { }

    public UserAddress(Guid userId, Address address, bool isPrimary = false)
    {
        UserId = userId;
        Address = address;
        IsPrimary = isPrimary;
    }

    public void UpdateAddress(Address address) => Address = address;

    public void SetPrimary() => IsPrimary = true;

    public void RemovePrimary() => IsPrimary = false;
}


using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.User;

public class User : AuditableEntity
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required string Phone { get; set; }

    public bool IsEmailVerified { get; set; }

    public required string Status { get; set; } // Active / Blocked 
	
	public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    private readonly List<UserAddress> _addresses = new();

    public IReadOnlyCollection<UserAddress> Addresses => _addresses;

    public void AddAddress(Address address, bool isPrimary = false)
    {
        if (isPrimary)
        {
            foreach (UserAddress existing in _addresses)
            {
                existing.RemovePrimary();
            }
        }

        _addresses.Add(new UserAddress(Id, address, isPrimary));
    }
}

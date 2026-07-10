using ECommerce.Domain.Common;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities.User;

public class User : AuditableEntity<Guid>
{
    public required string FirstName { get; set; } = string.Empty;

    public required string LastName { get; set; } = string.Empty;

    public required string Email { get; set; } = string.Empty;

    public required string PasswordHash { get; set; } = string.Empty;

    public required string Phone { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public required string Status { get; set; } // Active / Blocked 
         
    public string? OneTimePasswordSecret { get; set; }
    public bool IsTwoFactorEnabled { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];

    private readonly List<UserAddress> _addresses = [];

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

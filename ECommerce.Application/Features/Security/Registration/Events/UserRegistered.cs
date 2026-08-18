using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.Registration.Events;

public sealed record UserRegistered(
    Guid UserId,
    string Email,
    string FirstName) : IMessage;

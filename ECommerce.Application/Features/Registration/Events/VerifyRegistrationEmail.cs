using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Registration.Events;

public sealed record VerifyRegistrationEmail(
    Guid UserId,
    string Email,
    string FirstName) : IMessage;

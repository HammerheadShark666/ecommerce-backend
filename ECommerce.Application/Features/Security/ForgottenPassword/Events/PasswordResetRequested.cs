using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.ForgottenPassword.Events;

public sealed record PasswordResetRequested(
    Guid UserId,
    string FirstName,
    string Email) : IMessage;

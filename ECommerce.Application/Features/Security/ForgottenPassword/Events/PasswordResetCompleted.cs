using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.ForgottenPassword.Events;

public sealed record PasswordResetCompleted(
    Guid UserId,
    string FirstName,
    string Email,
    DateTime UpdatedAt) : IMessage;

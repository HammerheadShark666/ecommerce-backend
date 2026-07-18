using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.ForgottenPassword.Events;

public sealed record ResetPasswordRequested(
    Guid UserId,
    string FirstName,
    string Email) : IMessage;

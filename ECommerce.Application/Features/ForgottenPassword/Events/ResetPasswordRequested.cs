using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.ForgottenPassword.Events;

public sealed record ResetPasswordRequested(
    Guid UserId,
    string Email) : IMessage;

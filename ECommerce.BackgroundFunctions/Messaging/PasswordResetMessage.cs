namespace ECommerce.BackgroundFunctions.Messaging;
 
public sealed record PasswordResetRequestMessage(
    Guid Id,
    string Type,
    DateTimeOffset OccurredOn,
    PasswordResetRequestPayload Payload);

public sealed record PasswordResetRequestPayload(
    Guid UserId,
    string FirstName,
    string Email);

namespace ECommerce.BackgroundFunctions.Messaging;
 
public sealed record PasswordResetCompletedMessage(
    Guid Id,
    string Type,
    DateTimeOffset OccurredOn,
    PasswordResetCompletedPayload Payload);

public sealed record PasswordResetCompletedPayload(
    Guid UserId,
    string Email,
    string FirstName,
    DateTime UpdatedAt);

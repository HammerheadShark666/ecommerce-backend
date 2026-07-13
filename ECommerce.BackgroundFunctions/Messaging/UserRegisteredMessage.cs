namespace ECommerce.BackgroundFunctions.Messaging;

public sealed record UserRegisteredMessage(
    Guid Id,
    string Type,
    DateTimeOffset OccurredOn,
    UserRegisteredPayload Payload);

public sealed record UserRegisteredPayload(
    Guid UserId,
    string Email,
    string FirstName);

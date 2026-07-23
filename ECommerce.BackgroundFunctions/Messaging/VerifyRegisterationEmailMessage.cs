namespace ECommerce.BackgroundFunctions.Messaging;

public sealed record VerifyRegisterationEmailMessage(
    Guid Id,
    string Type,
    DateTimeOffset OccurredOn,
    VerifyRegisterationEmailPayload Payload);

public sealed record VerifyRegisterationEmailPayload(
    Guid UserId,
    string Email,
    string FirstName);

using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

public record ConfirmTwoFactorEnrolmentRequest(string Email, string Code);

public record ConfirmTwoFactorEnrolmentCommand(string Email, string Code) : ICommand<ConfirmTwoFactorEnrolmentResponse>;

public record ConfirmTwoFactorEnrolmentResponse(string Message);

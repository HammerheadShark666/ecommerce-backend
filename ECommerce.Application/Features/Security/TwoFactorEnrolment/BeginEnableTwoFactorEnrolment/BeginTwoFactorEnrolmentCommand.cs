using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;

public record BeginTwoFactorEnrolmentCommand(string Email) : ICommand<BeginTwoFactorEnrolmentResponse>;

public record BeginTwoFactorEnrolmentResponse(
    string QrCodeBase64, 
    string OtpAuthUri 
);

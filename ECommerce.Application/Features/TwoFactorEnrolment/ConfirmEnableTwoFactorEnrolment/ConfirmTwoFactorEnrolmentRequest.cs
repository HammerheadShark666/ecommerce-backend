namespace ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

public record ConfirmTwoFactorEnrolmentRequest(string email, string Code);

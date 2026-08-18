using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.Registration.BeginRegistration;

public record BeginRegistrationRequest(string Email, string Password, string ConfirmPassword, string LastName, string FirstName, string PhoneNumber);

public record BeginRegistrationCommand(string Email, string Password, string ConfirmPassword, string LastName, string FirstName, string PhoneNumber) : ICommand<BeginRegistrationResponse>;

public record BeginRegistrationResponse(string Message = "Registration initiated successfully. Email sent to verify email.");

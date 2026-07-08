namespace ECommerce.Application.Features.Registration.BeginRegistration;

public record BeginRegistrationRequest(string Email, string Password, string ConfirmPassword, string LastName, string FirstName, string PhoneNumber);

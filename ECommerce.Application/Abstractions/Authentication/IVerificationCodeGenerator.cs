namespace ECommerce.Application.Abstractions.Authentication;

public interface IVerificationCodeGenerator
{
    string Generate();
}

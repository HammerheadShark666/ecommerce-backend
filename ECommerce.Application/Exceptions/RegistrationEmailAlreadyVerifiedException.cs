namespace ECommerce.Application.Exceptions;

public class RegistrationEmailAlreadyVerifiedException : Exception
{
    public RegistrationEmailAlreadyVerifiedException()
        : base("The registration email has already been verified.")
    {
    }
}

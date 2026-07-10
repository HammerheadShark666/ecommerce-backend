namespace ECommerce.Application.Exceptions;

public class TwoFactorEnrolmentNotStartedException() : DomainException("TwoFactorEnrolment not started. Call /TwoFactorEnrolment first.")
{
}

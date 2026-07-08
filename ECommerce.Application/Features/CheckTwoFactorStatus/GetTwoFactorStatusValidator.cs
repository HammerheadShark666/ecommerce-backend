using FluentValidation;

namespace ECommerce.Application.Features.CheckTwoFactorStatus;
 
public class GetTwoFactorStatusValidator : AbstractValidator<GetTwoFactorStatusQuery>
{
    public GetTwoFactorStatusValidator() => RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
}

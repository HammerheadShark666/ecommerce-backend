using FluentValidation;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByEmail;
 
public class GetTwoFactorStatusByEmailQueryValidator : AbstractValidator<GetTwoFactorStatusByEmailQuery>
{
    public GetTwoFactorStatusByEmailQueryValidator() => RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
}

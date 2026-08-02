using ECommerce.Domain.ValueObjects;
using FluentValidation;

namespace ECommerce.Application.Common.Validators;

public class MoneyValidator : AbstractValidator<Money>
{
    public MoneyValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0)
            .WithMessage("Amount cannot be negative.");
        RuleFor(x => x.Currency).NotEmpty();
    }
}

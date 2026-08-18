using ECommerce.Application.Common.Validators;
using ECommerce.Domain.ValueObjects;
using FluentValidation;

namespace ECommerce.Application.Features.Commercial.Product.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.MinPrice!)
            .SetValidator(new MoneyValidator())
            .When(x => x.MinPrice is not null);

        RuleFor(x => x.MaxPrice!)
            .SetValidator(new MoneyValidator())
            .When(x => x.MaxPrice is not null); 

        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (!PriceRange.TryCreate(request.MinPrice, request.MaxPrice, out _, out string? error))
                {
                    context.AddFailure("PriceRange", error!);
                }
            }); 

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .WithMessage("Search term is too long.");
    }
}

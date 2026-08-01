using FluentValidation;

namespace ECommerce.Application.Features.Product.GetProduct; 

public sealed class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200);
    }
}

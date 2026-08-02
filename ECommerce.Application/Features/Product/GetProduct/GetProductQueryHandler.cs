using ECommerce.Application.Abstractions;
using ECommerce.Application.Common.Errors;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Product.GetProduct;

public sealed class GetProductQueryHandler(IECommerceDbContext dbContext)
    : IRequestHandler<GetProductQuery, Result<GetProductResult>>
{
    public async Task<Result<GetProductResult>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Product.Product? product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail<GetProductResult>(
                new NotFoundError($"Product '{request.Id}' was not found."));
        }

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice,
            product.StockQuantity); 

        bool slugMatches = string.Equals(product.Slug, request.Slug, StringComparison.Ordinal);

        return Result.Ok(new GetProductResult(response, slugMatches));
    }
}

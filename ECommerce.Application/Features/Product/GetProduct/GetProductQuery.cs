using FluentResults;
using MediatR;

namespace ECommerce.Application.Features.Product.GetProduct;

public sealed record GetProductQuery(Guid Id, string Slug) : IRequest<Result<GetProductResult>>;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price);
    //int StockQuantity);

public sealed record GetProductResult(ProductResponse Product, bool SlugMatches);

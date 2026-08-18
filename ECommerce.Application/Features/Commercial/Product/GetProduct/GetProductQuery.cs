using ECommerce.Domain.ValueObjects;
using FluentResults;
using MediatR;

namespace ECommerce.Application.Features.Commercial.Product.GetProduct;

public sealed record GetProductQuery(Guid Id, string Slug) : IRequest<Result<GetProductResult>>;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Money Price,
    int StockQuantity);

public sealed record GetProductResult(ProductResponse Product, bool SlugMatches);

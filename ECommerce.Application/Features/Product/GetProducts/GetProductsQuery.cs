using MediatR;

namespace ECommerce.Application.Features.Product.GetProducts;

// Query — sent to MediatR
public sealed record GetProductsQuery(
    int Page,
    int PageSize,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Search,
    ProductSortField? SortBy) : IRequest<GetProductsResponse>;

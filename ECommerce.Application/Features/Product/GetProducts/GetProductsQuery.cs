using FluentResults;
using MediatR;

namespace ECommerce.Application.Features.Product.GetProducts;

public sealed record GetProductsQuery(
    int Page,
    int PageSize,
    string? Category,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Search,
    ProductSortField? SortBy) : IRequest<Result<GetProductsResponse>>;

public sealed class GetProductsRequest
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Category { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Search { get; init; }
    public ProductSortField? SortBy { get; init; }
}

public sealed record GetProductsResponse(
    IReadOnlyList<ProductDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record ProductDto(
    Guid Id,
    string Name,
    // string Sku,
    decimal Price,
    // int Stock,
    string? CategoryName,
    bool IsActive);


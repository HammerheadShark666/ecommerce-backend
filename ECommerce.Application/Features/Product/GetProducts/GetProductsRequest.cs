namespace ECommerce.Application.Features.Product.GetProducts;

// Request — bound from query string via [AsParameters]
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

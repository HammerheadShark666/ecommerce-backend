namespace ECommerce.Application.Features.Product.GetProducts;

// Response
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

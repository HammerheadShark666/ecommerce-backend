using ECommerce.Application.Abstractions;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Product.GetProducts;

public sealed class GetProductsQueryHandler(IECommerceDbContext dbContext) : IRequestHandler<GetProductsQuery, Result<GetProductsResponse>>
{ 
    public async Task<Result<GetProductsResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (request.Category is not null)
        {
            query = query.Where(p => p.Category != null && p.Category.Name == request.Category);
        }

        if (request.MinPrice is not null)
        {
            query = query.Where(p =>
                p.BasePrice.Currency == request.MinPrice.Currency &&
                p.BasePrice.Amount >= request.MinPrice.Amount);
        }

        if (request.MaxPrice is not null)
        {
            query = query.Where(p =>
                p.BasePrice.Currency == request.MaxPrice.Currency &&
                p.BasePrice.Amount <= request.MaxPrice.Amount);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{request.Search}%"));
        }

        query = ApplySort(query, request.SortBy);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name, 
                p.BasePrice,
                p.StockQuantity,
                p.Category != null ? p.Category.Name : null,
                p.IsActive))
            .ToListAsync(cancellationToken); 
       
        return Result.Ok(new GetProductsResponse(items, totalCount, request.Page, request.PageSize));
    }

    private static IQueryable<Domain.Entities.Product.Product> ApplySort(
    IQueryable<Domain.Entities.Product.Product> query, ProductSortField? sortBy) => sortBy switch
    {
        ProductSortField.Name => query.OrderBy(p => p.Name),
        ProductSortField.Price => query.OrderBy(p => p.BasePrice.Amount),
        ProductSortField.Stock => query.OrderBy(p => p.StockQuantity),
        ProductSortField.CreatedAt => query.OrderByDescending(p => p.CreatedAt),
        _ => query.OrderBy(p => p.Name)
    };
}

using ECommerce.Application.Abstractions;
using ECommerce.Application.Features.Product.GetProduct;
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
        IQueryable<Domain.Entities.Product.Product> query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category != null
                                         && p.Category.Name == request.Category);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{request.Search}%"));
        }

        query = ApplySort(query, request.SortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ProductDto> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name, 
                p.BasePrice,
                //p.Stock,
                p.Category != null ? p.Category.Name : null,
                p.IsActive))
            .ToListAsync(cancellationToken);

        //return new GetProductsResponse(items, totalCount, request.Page, request.PageSize);

        return Result.Ok(new GetProductsResponse(items, totalCount, request.Page, request.PageSize));
    }

    private static IQueryable<Domain.Entities.Product.Product> ApplySort(IQueryable<Domain.Entities.Product.Product> query, ProductSortField? sortBy) => sortBy switch
    {
        ProductSortField.Name => query.OrderBy(p => p.Name),
        ProductSortField.Price => query.OrderBy(p => p.BasePrice),
        //ProductSortField.Stock => query.OrderBy(p => p.Stock),
        ProductSortField.CreatedAt => query.OrderByDescending(p => p.CreatedAt),
        _ => query.OrderBy(p => p.Name)
    };
}

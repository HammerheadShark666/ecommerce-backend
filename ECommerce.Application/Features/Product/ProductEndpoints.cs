using ECommerce.Application.Features.Product.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Product;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/products")
                             .WithTags("Products");

        group.MapGet("", async ([AsParameters] GetProductsRequest request, IMediator mediator) =>
        {
            GetProductsResponse result = await mediator.Send(new GetProductsQuery(request.Page ?? 1, 
                                                                                  request.PageSize ?? 20, 
                                                                                  request.Category,
                                                                                  request.MinPrice, 
                                                                                  request.MaxPrice, 
                                                                                  request.Search, 
                                                                                  request.SortBy));
            return Results.Ok(result);
        });

        return endpoints;
    }
}

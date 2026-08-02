using ECommerce.Application.Common;
using ECommerce.Application.Constants;
using ECommerce.Application.Features.Product.GetProduct;
using ECommerce.Application.Features.Product.GetProducts;
using ECommerce.Domain.ValueObjects;
using FluentResults;
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
            Result<GetProductsResponse> result = await mediator.Send(new GetProductsQuery(request.Page ?? 1,
                                                                                          request.PageSize ?? 20,
                                                                                          request.Category,
                                                                                          request.MinPrice.HasValue ? new Money(request.MinPrice.Value, CurrencyConstants.CurrencyGBPound) : null,
                                                                                          request.MaxPrice.HasValue ? new Money(request.MaxPrice.Value, CurrencyConstants.CurrencyGBPound) : null,
                                                                                          request.Search,
                                                                                          request.SortBy));
            if (result.IsFailed)
            {
                return result.ToHttpResult();
            }

            return Results.Ok(result.Value);
        })
        .WithName("GetProducts")
        .WithTags("Products");

        group.MapGet("/{id:guid}/{slug}", async (Guid id, string slug, ISender sender, CancellationToken cancellationToken) =>
        {
            Result<GetProductResult> result = await sender.Send(new GetProductQuery(id, slug), cancellationToken);

            if (result.IsFailed)
            {
                return result.ToHttpResult();
            }

            if (!result.Value.SlugMatches)
            {
                return Results.Redirect(
                    $"/products/{result.Value.Product.Id}/{result.Value.Product.Slug}",
                    permanent: true);
            }

            return Results.Ok(result.Value.Product);
        })
        .WithName("GetProduct")
        .WithTags("Products")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status301MovedPermanently);

        return endpoints;
    }
}

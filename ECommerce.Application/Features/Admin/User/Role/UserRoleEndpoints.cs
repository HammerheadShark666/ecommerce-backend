using ECommerce.Application.Constants;
using ECommerce.Application.Features.Commercial.Product.AddProduct;
using ECommerce.Application.Features.Commercial.Product.GetProduct;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Application.Features.Admin.User.Role;
 
public static class UserRoleEndpoints
{
    public static IEndpointRouteBuilder MapUserRoleEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/user-role")
                             .WithTags("UserRole");

        group.MapPost("/add", async (AddUserRoleRequest command, IMediator mediator) =>
        {
            var result = await mediator.Send(new AddUserRoleCommand(command.UserId, command.RoleId));           
            return Results.Ok(result);
        })
        .WithName("AddUserRole")
        .WithTags("UserRoles")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .RequireAuthorization(PolicyNamesConstants.CanManageUsers);

        return endpoints;
    }
}
